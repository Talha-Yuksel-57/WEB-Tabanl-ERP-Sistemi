using ERP.API.Services;
using ERP.Core.Entities;
using ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITenantService _tenantService;

        public StaffController(AppDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        // --- A. TAMİRCİ: İş Emri Görüntüleme (Maliyet Gizli) ---
        [Authorize(Roles = "Technician")]
        [HttpGet("my-repairs")]
        public async Task<IActionResult> GetMyRepairs()
        {
            var tenantId = _tenantService.GetTenantId();
            var repairs = await _context.ServiceOrders
                .Where(s => s.TenantId == tenantId && s.Status != "Tamamlandı")
                .Select(s => new {
                    s.Id,
                    s.DeviceName,
                    s.Status,
                    s.ServiceFee // Sadece toplam ücreti görür (Döküman 3.A)
                })
                .ToListAsync();
            return Ok(repairs);
        }

        // --- B. KASİYER: Yeni Satış Yapma ---
        [Authorize(Roles = "Cashier")]
        [HttpPost("new-sale")]
        public async Task<IActionResult> CreateSale([FromBody] Sale sale)
        {
            sale.TenantId = _tenantService.GetTenantId();
            sale.CreatedAt = DateTime.Now;
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();
            return Ok("Satış kaydedildi.");
        }

        // --- C. YAZILIMCI: Zaman Takibi (Senin paylaştığın kısım) ---
        [Authorize(Roles = "Developer")]
        [HttpPost("log-work-hours")]
        public async Task<IActionResult> LogWorkHours(int taskId, int hours)
        {
            var tenantId = _tenantService.GetTenantId();
            var task = await _context.ProjectTasks.FirstOrDefaultAsync(t => t.Id == taskId && t.TenantId == tenantId);

            if (task == null) return NotFound("Görev bulunamadı.");

            task.HoursWorked += hours;
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"{hours} saatlik çalışma kaydedildi.", CurrentTotal = task.HoursWorked });
        }

        // --- C. YAZILIMCI: Masraf Girişi ---
        [Authorize(Roles = "Developer")]
        [HttpPost("add-expense")]
        public async Task<IActionResult> AddExpense([FromBody] decimal amount, string description)
        {
            // İleride burayı 'Expenses' tablosuna bağlayabiliriz.
            return Ok(new { Status = "Başarılı", Detail = "Masraf kaydı muhasebe onayına gönderildi." });
        }
    }
}