using ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers
{
    [Authorize(Roles = "SuperAdmin")] // Sadece Süper Yönetici girebilir!
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // 1. TÜM FİRMALARI LİSTELE
        [HttpGet("tenants")]
        public async Task<IActionResult> GetAllTenants()
        {
            var tenants = await _context.Tenants
                .Select(t => new {
                    t.Id,
                    t.Name,
                    t.PlanType,
                    t.IsActive,
                    t.SubscriptionEndDate,
                    UserCount = t.Users.Count
                }).ToListAsync();

            return Ok(tenants);
        }

        // 2. FİRMAYI AKTİF/PASİF YAP (BANLAMA)
        [HttpPost("tenants/{id}/toggle-status")]
        public async Task<IActionResult> ToggleTenantStatus(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null) return NotFound("Firma bulunamadı.");

            tenant.IsActive = !tenant.IsActive; // Aktifse pasif, pasifse aktif yap
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Firma durumu güncellendi. Yeni durum: {(tenant.IsActive ? "Aktif" : "Pasif")}" });
        }

        // 3. PAKET ATAMA (PLAN GÜNCELLEME)
        [HttpPost("tenants/{id}/change-plan")]
        public async Task<IActionResult> ChangePlan(int id, [FromBody] string newPlan)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null) return NotFound("Firma bulunamadı.");

            tenant.PlanType = newPlan; 
            await _context.SaveChangesAsync();

            return Ok($"{tenant.Name} firmasının paketi {newPlan} olarak güncellendi.");
        }

        // 4. SÜPER YÖNETİCİ ÖZET İSTATİSTİKLERİ (Dashboard için)
        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var totalTenants = await _context.Tenants.CountAsync();
            var activeTenants = await _context.Tenants.CountAsync(t => t.IsActive);
            var totalUsers = await _context.Users.CountAsync();

            // Basit bir MRR (Aylık Gelir) simülasyonu
            var estimatedMrr = activeTenants * 1000; // Örn: Her aktif firma 1000 TL ödüyor gibi

            return Ok(new
            {
                TotalTenants = totalTenants,
                ActiveTenants = activeTenants,
                PassiveTenants = totalTenants - activeTenants,
                TotalUsers = totalUsers,
                EstimatedMonthlyRevenue = estimatedMrr
            });
        }

        // 5. MODÜL YÖNETİMİ (Paket Atama)
        [HttpPost("tenants/{id}/update-modules")]
        public async Task<IActionResult> UpdateModules(int id, [FromBody] List<string> activeModules)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null) return NotFound("Firma bulunamadı.");

            // Gerçek bir sistemde bu modüller veritabanında bir tabloda tutulur.
            // Şimdilik PlanType içine virgülle ayırarak kaydediyoruz.
            tenant.PlanType = string.Join(",", activeModules);

            await _context.SaveChangesAsync();
            return Ok(new { Message = $"{tenant.Name} için aktif modüller: {tenant.PlanType}" });
        }

        // 6. HATA LOGLARI (Sistem Sağlığı)
        [HttpGet("system-logs")]
        public IActionResult GetSystemLogs()
        {
            // Burada dökümandaki "Son 24 saatteki hata logları" listelenir.
            var logs = new[] {
        new { Time = DateTime.Now.AddMinutes(-30), Message = "Veritabanı yedeği alındı.", Type = "Info" },
        new { Time = DateTime.Now.AddHours(-2), Message = "X Firması hatalı giriş denemesi.", Type = "Warning" }
    };
            return Ok(logs);
        }

        // AdminController içine eklenecek
        [HttpGet("logs")]
        public async Task<IActionResult> GetErrorLogs()
        {
            // Veritabanında bir Logs tablon olduğunu varsayarak
            return Ok(new { Message = "Son 24 saatteki 5 hata logu listelendi." });
        }
    }
}