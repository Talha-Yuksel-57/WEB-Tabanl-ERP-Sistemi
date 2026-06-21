using ERP.Core.DTOs.Report;

namespace ERP.Core.Interfaces
{
    public interface IReportService
    {
        // --- EXPORT ---
        Task<byte[]> ExportSalesToPdfAsync(SaleReportFilterDto filter);
        Task<byte[]> ExportSalesToExcelAsync(SaleReportFilterDto filter);
        Task<byte[]> ExportStockToPdfAsync();
        Task<byte[]> ExportStockToExcelAsync();

        // --- IMPORT ---
        Task<ImportResultDto> ImportProductsFromExcelAsync(Stream fileStream);
        Task<ImportResultDto> ImportProductsFromJsonAsync(Stream fileStream);
        Task<ImportResultDto> ImportProductsFromXmlAsync(Stream fileStream);

        // Veri hazırlama (PDF/Excel ikisi de kullanır)
        Task<List<SaleReportRowDto>> GetSaleReportDataAsync(SaleReportFilterDto filter);
        Task<List<StockReportRowDto>> GetStockReportDataAsync();
    }
}
