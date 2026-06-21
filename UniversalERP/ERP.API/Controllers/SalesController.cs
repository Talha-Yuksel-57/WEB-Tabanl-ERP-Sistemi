using ERP.Core.DTOs.Sale;
using ERP.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SalesController : ControllerBase
    {
        private readonly ISaleService _saleService;

        public SalesController(ISaleService saleService)
        {
            _saleService = saleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sales = await _saleService.GetAllAsync();
            return Ok(sales);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var sale = await _saleService.GetByIdAsync(id);
            if (sale == null) return NotFound("Satış bulunamadı.");
            return Ok(sale);
        }

        /// <summary>
        /// Satış oluşturur — Stok düşürme + Satış kaydı atomik olarak gerçekleşir (ACID)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,TenantAdmin,Manager,Cashier")]
        public async Task<IActionResult> Create([FromBody] CreateSaleDto dto)
        {
            // Token'dan kasiyer ID'sini al
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int cashierId = int.TryParse(userIdStr, out var parsed) ? parsed : 0;

            var sale = await _saleService.CreateSaleAsync(dto, cashierId);
            return CreatedAtAction(nameof(GetById), new { id = sale.Id }, sale);
        }

        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin,Manager")]
        public async Task<IActionResult> Cancel(int id)
        {
            var success = await _saleService.CancelSaleAsync(id);
            if (!success) return BadRequest("Satış iptal edilemedi veya zaten iptal edilmiş.");
            return Ok(new { message = "Satış iptal edildi, stok iade edildi." });
        }
    }
}
