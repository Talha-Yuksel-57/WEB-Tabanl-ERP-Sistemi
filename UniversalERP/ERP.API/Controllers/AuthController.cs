using ERP.Core.DTOs.Auth;
using ERP.Core.DTOs.Customer;
using ERP.Core.Entities;
using ERP.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly ICustomerService _customerService;

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            ICustomerService customerService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
            _customerService = customerService;
        }

        /// <summary>Yeni kullanıcı kaydı</summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var user = new AppUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                TenantId = dto.TenantId,
                Role = dto.Role,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Kayıt başarısız: {Email} | Hatalar: {Errors}",
                    dto.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            await _userManager.AddToRoleAsync(user, dto.Role);

            // ✅ Rol "Customer" ise otomatik Customer kaydı oluştur
            if (dto.Role?.Equals("Customer", StringComparison.OrdinalIgnoreCase) == true)
            {
                try
                {
                    var createCustomerDto = new CreateCustomerDto
                    {
                        FullName = dto.FullName,
                        Email = dto.Email,
                        Phone = null,
                        Address = null
                    };
                    
                    await _customerService.CreateAsync(createCustomerDto);
                    _logger.LogInformation("Müşteri kaydı otomatik oluşturuldu: {Email}", dto.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Müşteri kaydı oluştururken hata: {Email}", dto.Email);
                    // AppUser başarılı, işlem devam etsin
                }
            }

            _logger.LogInformation("Yeni kullanıcı kaydedildi: {Email} | Rol: {Role} | TenantId: {TenantId}",
                dto.Email, dto.Role, dto.TenantId);

            return Ok(new { message = "Kullanıcı başarıyla oluşturuldu.", userId = user.Id });
        }

        /// <summary>Giriş yap — AccessToken + RefreshToken döner</summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("Başarısız giriş denemesi: {Email}", dto.Email);
                return Unauthorized("Hatalı e-posta veya şifre.");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Pasif hesaba giriş denemesi: {Email}", dto.Email);
                return Unauthorized("Hesabınız aktif değil. Yönetici ile iletişime geçin.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Yanlış şifre: {Email}", dto.Email);
                return Unauthorized("Hatalı e-posta veya şifre.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = GenerateAccessToken(user, roles);
            var refreshToken = GenerateRefreshToken();

            // RefreshToken'ı kullanıcıya kaydet
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(
                int.Parse(_configuration["Jwt:RefreshTokenExpireDays"] ?? "7"));
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Giriş başarılı: {Email} | Rol: {Role} | TenantId: {TenantId}",
                user.Email, string.Join(",", roles), user.TenantId);

            return Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(
                    int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60")),
                FullName = user.FullName,
                Email = user.Email!,
                Role = roles.FirstOrDefault() ?? "Employee",
                TenantId = user.TenantId
            });
        }

        /// <summary>AccessToken süresi dolunca yenile</summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
        {
            var user = _userManager.Users
                .FirstOrDefault(u => u.RefreshToken == dto.RefreshToken);

            if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            {
                _logger.LogWarning("Geçersiz refresh token kullanımı.");
                return Unauthorized("Oturum süresi doldu. Lütfen tekrar giriş yapın.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = GenerateAccessToken(user, roles);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(
                int.Parse(_configuration["Jwt:RefreshTokenExpireDays"] ?? "7"));
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Token yenilendi: {Email}", user.Email);

            return Ok(new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(
                    int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60")),
                FullName = user.FullName,
                Email = user.Email!,
                Role = roles.FirstOrDefault() ?? "Employee",
                TenantId = user.TenantId
            });
        }

        /// <summary>Çıkış yap — RefreshToken geçersiz kıl</summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;
                await _userManager.UpdateAsync(user);
                _logger.LogInformation("Çıkış yapıldı: {Email}", user.Email);
            }

            return Ok(new { message = "Başarıyla çıkış yapıldı." });
        }

        // --- YARDIMCI METODLAR ---

        private string GenerateAccessToken(AppUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email!),
                new("TenantId", user.TenantId.ToString()),
                new("FullName", user.FullName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Tüm rolleri claim olarak ekle
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60");

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
