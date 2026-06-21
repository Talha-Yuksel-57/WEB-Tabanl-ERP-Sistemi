namespace ERP.Core.DTOs.Dashboard
{
    // Ana dashboard — tüm KPI kartları
    public class DashboardDto
    {
        // Sayılar
        public int TotalProducts { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalSalesToday { get; set; }
        public int TotalSalesThisMonth { get; set; }
        public int OpenServiceOrders { get; set; }
        public int PendingTasks { get; set; }
        public int LowStockProductCount { get; set; }

        // Gelir
        public decimal TodayRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public decimal LastMonthRevenue { get; set; }

        // Son işlemler
        public List<RecentSaleDto> RecentSales { get; set; } = new();
        public List<LowStockProductDto> LowStockProducts { get; set; } = new();

        // Grafik verileri
        public List<MonthlySaleDto> MonthlySales { get; set; } = new();
        public List<TopProductDto> TopProducts { get; set; } = new();
    }

    public class RecentSaleDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class LowStockProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int StockCount { get; set; }
        public int MinStockLevel { get; set; }
    }

    // Aylık satış grafiği için
    public class MonthlySaleDto
    {
        public string Month { get; set; } = string.Empty;  // "Ocak", "Şubat"...
        public int Year { get; set; }
        public decimal Revenue { get; set; }
        public int SaleCount { get; set; }
    }

    // En çok satılan ürünler
    public class TopProductDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
