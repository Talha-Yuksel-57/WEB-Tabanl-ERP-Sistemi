namespace ERP.Core.DTOs.Report
{
    // Satış raporu için filtre parametreleri
    public class SaleReportFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }         // Completed, Cancelled
        public string? PaymentMethod { get; set; }  // Nakit, Kredi Kartı, Havale
    }

    // Satış raporu satırı
    public class SaleReportRowDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
    }

    // Stok raporu satırı
    public class StockReportRowDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockCount { get; set; }
        public int MinStockLevel { get; set; }
        public bool IsLowStock { get; set; }
        public bool IsActive { get; set; }
    }

    // Excel/JSON import için ürün satırı
    public class ProductImportRowDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockCount { get; set; }
        public int MinStockLevel { get; set; } = 5;
    }

    // Import sonuç özeti
    public class ImportResultDto
    {
        public int TotalRows { get; set; }
        public int Imported { get; set; }
        public int Skipped { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
