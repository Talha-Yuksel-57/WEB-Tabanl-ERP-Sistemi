using ERP.API.Services;      // ITenantService için doğru adres
using ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers
{
    [Authorize(Roles = "TenantAdmin")] // Sadece firma patronu görebilir
    [Route("api/[controller]")]
    [ApiController]
    public class TenantDashboardController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITenantService _tenantService;

        public TenantDashboardController(AppDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetBusinessStats()
        {
            // O an giriş yapmış olan kullanıcının TenantId'sini otomatik alır
            var tenantId = _tenantService.GetTenantId();

            // 1. Tamirci Senaryosu: Bekleyen Cihazlar
            var pendingRepairs = await _context.ServiceOrders
                .CountAsync(s => s.TenantId == tenantId && s.Status != "Tamamlandı");

            // 2. Market Senaryosu: Bugünkü Ciro
            var todayCiro = await _context.Sales
                .Where(s => s.TenantId == tenantId && s.CreatedAt.Date == DateTime.Today)
                .SumAsync(s => s.TotalAmount);

            // 3. Genel: Kritik Stok Uyarıları (Stok < 5 olan ürünler)
            var lowStockCount = await _context.Products
                .CountAsync(p => p.TenantId == tenantId && p.StockCount < 5);

            return Ok(new
            {
                PendingRepairs = pendingRepairs,
                DailyRevenue = todayCiro,
                CriticalStockCount = lowStockCount,
                LastUpdate = DateTime.Now
            });
        }

        // PERSONEL LİSTESİ
        [HttpGet("staff")]
        public async Task<IActionResult> GetStaffList()
        {
            var tenantId = _tenantService.GetTenantId();
            var staff = await _context.Users
                .Where(u => u.TenantId == tenantId)
                .Select(u => new { u.Id, u.UserName, u.Email })
                .ToListAsync();

            return Ok(staff);
        }

        // PERSONELİ PASİFE AL (Dökümandaki "Çıkar" mantığı)
        [HttpDelete("staff/{id}")]
        public async Task<IActionResult> RemoveStaff(string id)
        {
            var tenantId = _tenantService.GetTenantId();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId);

            if (user == null) return NotFound("Personel bulunamadı.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok("Personel başarıyla silindi.");
        }
    }
}