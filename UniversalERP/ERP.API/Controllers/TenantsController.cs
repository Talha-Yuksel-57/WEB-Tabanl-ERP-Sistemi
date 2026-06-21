using ERP.Core.DTOs.Profile;
using ERP.Core.Entities;
using ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TenantsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Aktif firmaları listeler. Kayıt formunda firma seçimi için
        /// kimlik doğrulaması gerektirmez (Register sayfasında kullanılır).
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetTenants()
        {
            var tenants = await _context.Tenants
                .IgnoreQueryFilters()
                .Where(t => t.IsActive)
                .Select(t => new { t.Id, t.Name })
                .ToListAsync();
            return Ok(tenants);
        }

        /// <summary>Yeni firma oluşturur (SuperAdmin işlemi)</summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateTenant(Tenant tenant)
        {
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();
            return Ok(tenant);
        }

        /// <summary>Giriş yapan kullanıcının firma ayarlarını döner</summary>
        [HttpGet("settings")]
        [Authorize]
        public async Task<IActionResult> GetSettings()
        {
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            int.TryParse(tenantIdClaim, out var tenantId);

            var tenant = await _context.Tenants
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null) return NotFound("Firma bulunamadı.");

            return Ok(new TenantSettingsDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Address = tenant.Address,
                TaxNumber = tenant.TaxNumber,
                Industry = tenant.Industry,
                PlanType = tenant.PlanType
            });
        }

        /// <summary>Firma ayarlarını güncelle (sadece TenantAdmin ve SuperAdmin)</summary>
        [HttpPut("settings")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin")]
        public async Task<IActionResult> UpdateSettings([FromBody] UpdateTenantSettingsDto dto)
        {
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            int.TryParse(tenantIdClaim, out var tenantId);

            var tenant = await _context.Tenants
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null) return NotFound("Firma bulunamadı.");

            tenant.Name = dto.Name;
            tenant.Address = dto.Address;
            tenant.TaxNumber = dto.TaxNumber;
            tenant.Industry = dto.Industry;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Firma ayarları güncellendi." });
        }
    }
}
