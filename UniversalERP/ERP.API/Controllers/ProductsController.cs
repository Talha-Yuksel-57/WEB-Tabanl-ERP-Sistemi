using ERP.Core.DTOs.Product;
using ERP.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    /// <summary>Ürün yönetimi işlemleri</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>Tenant'a ait tüm ürünleri listeler</summary>
        /// <returns>Ürün listesi</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        /// <summary>ID'ye göre ürün getirir</summary>
        /// <param name="id">Ürün ID</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound("Ürün bulunamadı.");
            return Ok(product);
        }

        /// <summary>Stok kritik seviyenin altındaki ürünleri listeler</summary>
        [HttpGet("low-stock")]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), 200)]
        public async Task<IActionResult> GetLowStock()
        {
            var products = await _productService.GetLowStockAsync();
            return Ok(products);
        }

        /// <summary>Yeni ürün oluşturur</summary>
        /// <remarks>Sadece Manager ve üzeri roller kullanabilir.</remarks>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,TenantAdmin,Manager")]
        [ProducesResponseType(typeof(ProductDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            var product = await _productService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        /// <summary>Ürün bilgilerini günceller</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin,Manager")]
        [ProducesResponseType(typeof(ProductDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
            var product = await _productService.UpdateAsync(id, dto);
            if (product == null) return NotFound("Ürün bulunamadı.");
            return Ok(product);
        }

        /// <summary>Ürün stok miktarını günceller</summary>
        [HttpPatch("{id}/stock")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin,Manager,Cashier")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockDto dto)
        {
            var success = await _productService.UpdateStockAsync(id, dto.NewStock);
            if (!success) return NotFound("Ürün bulunamadı.");
            return Ok(new { message = "Stok güncellendi." });
        }

        /// <summary>Ürünü soft-delete ile siler (veritabanından kaldırılmaz)</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _productService.DeleteAsync(id);
            if (!success) return NotFound("Ürün bulunamadı.");
            return Ok(new { message = "Ürün silindi." });
        }
    }
}
