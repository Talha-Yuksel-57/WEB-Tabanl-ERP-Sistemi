using ERP.Core.DTOs.Profile;
using ERP.Core.Entities;
using ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            UserManager<AppUser> userManager,
            AppDbContext context,
            ILogger<ProfileController> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        /// <summary>Giriş yapan kullanıcının profil bilgilerini döner</summary>
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            var roles = await _userManager.GetRolesAsync(user);
            var tenant = await _context.Tenants.IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id == user.TenantId);

            return Ok(new ProfileDto
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FullName = user.FullName,
                Department = user.Department ?? "",
                Role = roles.FirstOrDefault() ?? "",
                TenantId = user.TenantId,
                TenantName = tenant?.Name ?? ""
            });
        }

        /// <summary>Profil bilgilerini güncelle (Ad Soyad, Departman)</summary>
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            user.FullName = dto.FullName;
            user.Department = dto.Department ?? user.Department;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            _logger.LogInformation("Profil güncellendi: {Email}", user.Email);
            return Ok(new { message = "Profil güncellendi." });
        }

        /// <summary>Şifre değiştir</summary>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            var result = await _userManager.ChangePasswordAsync(
                user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Şifre değiştirme başarısız: {Email}", user.Email);
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            _logger.LogInformation("Şifre değiştirildi: {Email}", user.Email);
            return Ok(new { message = "Şifre başarıyla değiştirildi." });
        }
    }
}
