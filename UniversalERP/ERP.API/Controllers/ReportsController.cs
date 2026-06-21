using ERP.Core.DTOs.Report;
using ERP.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────
        // PDF EXPORT
        // ─────────────────────────────────────────────────────

        /// <summary>Satış raporunu PDF olarak indir</summary>
        [HttpGet("sales/pdf")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin,Manager")]
        public async Task<IActionResult> ExportSalesToPdf(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? status,
            [FromQuery] string? paymentMethod)
        {
            var filter = new SaleReportFilterDto
            {
                StartDate = startDate,
                EndDate = endDate,
                Status = status,
                PaymentMethod = paymentMethod
            };

            var pdfBytes = await _reportService.ExportSalesToPdfAsync(filter);
            var fileName = $"satis-raporu-{DateTime.Now:yyyyMMdd-HHmm}.pdf";

            _logger.LogInformation("Satış PDF raporu oluşturuldu: {FileName}", fileName);
            return File(pdfBytes, "application/pdf", fileName);
        }

        /// <summary>Stok raporunu PDF olarak indir</summary>
        [HttpGet("stock/pdf")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin,Manager")]
        public async Task<IActionResult> ExportStockToPdf()
        {
            var pdfBytes = await _reportService.ExportStockToPdfAsync();
            var fileName = $"stok-raporu-{DateTime.Now:yyyyMMdd-HHmm}.pdf";

            _logger.LogInformation("Stok PDF raporu oluşturuldu: {FileName}", fileName);
            return File(pdfBytes, "application/pdf", fileName);
        }

        // ─────────────────────────────────────────────────────
        // EXCEL EXPORT
        // ─────────────────────────────────────────────────────

        /// <summary>Satış raporunu Excel olarak indir</summary>
        [HttpGet("sales/excel")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin,Manager")]
        public async Task<IActionResult> ExportSalesToExcel(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? status,
            [FromQuery] string? paymentMethod)
        {
            var filter = new SaleReportFilterDto
            {
                StartDate = startDate,
                EndDate = endDate,
                Status = status,
                PaymentMethod = paymentMethod
            };

            var excelBytes = await _reportService.ExportSalesToExcelAsync(filter);
            var fileName = $"satis-raporu-{DateTime.Now:yyyyMMdd-HHmm}.xlsx";

            _logger.LogInformation("Satış Excel raporu oluşturuldu: {FileName}", fileName);
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        /// <summary>Stok raporunu Excel olarak indir</summary>
        [HttpGet("stock/excel")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin,Manager")]
        public async Task<IActionResult> ExportStockToExcel()
        {
            var excelBytes = await _reportService.ExportStockToExcelAsync();
            var fileName = $"stok-raporu-{DateTime.Now:yyyyMMdd-HHmm}.xlsx";

            _logger.LogInformation("Stok Excel raporu oluşturuldu: {FileName}", fileName);
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        // ─────────────────────────────────────────────────────
        // IMPORT
        // ─────────────────────────────────────────────────────

        /// <summary>
        /// Excel'den ürün aktar.
        /// Beklenen sütunlar: A=Ad, B=Fiyat, C=Stok, D=MinStok (1. satır başlık)
        /// </summary>
        [HttpPost("products/import/excel")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin,Manager")]
        public async Task<IActionResult> ImportProductsFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Dosya seçilmedi.");

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Sadece .xlsx dosyaları kabul edilmektedir.");

            using var stream = file.OpenReadStream();
            var result = await _reportService.ImportProductsFromExcelAsync(stream);

            _logger.LogInformation("Excel import: {Imported} ürün eklendi, {Skipped} atlandı.",
                result.Imported, result.Skipped);

            return Ok(result);
        }

        /// <summary>
        /// JSON'dan ürün aktar.
        /// Format: [{"name":"Ürün","price":10.5,"stockCount":100,"minStockLevel":5}]
        /// </summary>
        [HttpPost("products/import/json")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin,Manager")]
        public async Task<IActionResult> ImportProductsFromJson(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Dosya seçilmedi.");

            if (!file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Sadece .json dosyaları kabul edilmektedir.");

            using var stream = file.OpenReadStream();
            var result = await _reportService.ImportProductsFromJsonAsync(stream);

            _logger.LogInformation("JSON import: {Imported} ürün eklendi, {Skipped} atlandı.",
                result.Imported, result.Skipped);

            return Ok(result);
        }

        /// <summary>
        /// XML'den ürün aktar.
        /// Format: &lt;Products&gt;&lt;ProductImportRowDto&gt;...&lt;/ProductImportRowDto&gt;&lt;/Products&gt;
        /// </summary>
        [HttpPost("products/import/xml")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin,Manager")]
        public async Task<IActionResult> ImportProductsFromXml(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Dosya seçilmedi.");

            if (!file.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Sadece .xml dosyaları kabul edilmektedir.");

            using var stream = file.OpenReadStream();
            var result = await _reportService.ImportProductsFromXmlAsync(stream);

            _logger.LogInformation("XML import: {Imported} ürün eklendi, {Skipped} atlandı.",
                result.Imported, result.Skipped);

            return Ok(result);
        }

        // ─────────────────────────────────────────────────────
        // VERİ ÖNIZLEME (rapor indirmeden önce görmek için)
        // ─────────────────────────────────────────────────────

        /// <summary>Satış raporu verilerini JSON olarak döner</summary>
        [HttpGet("sales/preview")]
        public async Task<IActionResult> PreviewSales(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? status)
        {
            var filter = new SaleReportFilterDto
            {
                StartDate = startDate,
                EndDate = endDate,
                Status = status
            };
            var data = await _reportService.GetSaleReportDataAsync(filter);
            return Ok(new
            {
                count = data.Count,
                totalRevenue = data.Where(r => r.Status == "Completed").Sum(r => r.TotalAmount),
                rows = data
            });
        }

        /// <summary>Stok raporu verilerini JSON olarak döner</summary>
        [HttpGet("stock/preview")]
        public async Task<IActionResult> PreviewStock()
        {
            var data = await _reportService.GetStockReportDataAsync();
            return Ok(new
            {
                count = data.Count,
                lowStockCount = data.Count(r => r.IsLowStock),
                rows = data
            });
        }
    }
}
