using ERP.Core.DTOs.ServiceOrder;
using ERP.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ServiceOrdersController : ControllerBase
    {
        private readonly IServiceOrderService _serviceOrderService;

        public ServiceOrdersController(IServiceOrderService serviceOrderService)
        {
            _serviceOrderService = serviceOrderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _serviceOrderService.GetAllAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _serviceOrderService.GetByIdAsync(id);
            if (order == null) return NotFound("Servis kaydı bulunamadı.");
            return Ok(order);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,TenantAdmin,Manager,Technician")]
        public async Task<IActionResult> Create([FromBody] CreateServiceOrderDto dto)
        {
            var order = await _serviceOrderService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin,Manager,Technician")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateServiceOrderStatusDto dto)
        {
            var success = await _serviceOrderService.UpdateStatusAsync(id, dto.Status);
            if (!success) return NotFound("Servis kaydı bulunamadı.");
            return Ok(new { message = $"Durum '{dto.Status}' olarak güncellendi." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _serviceOrderService.DeleteAsync(id);
            if (!success) return NotFound("Servis kaydı bulunamadı.");
            return Ok(new { message = "Servis kaydı silindi." });
        }
    }
}
