using ERP.Core.DTOs.Dashboard;
using ERP.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Data.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardDto> GetDashboardAsync()
        {
            var today = DateTime.Today;
            var thisMonthStart = new DateTime(today.Year, today.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);

            // --- KPI SAYILARI ---
            var totalProducts = await _context.Products.CountAsync();
            var totalCustomers = await _context.Customers.CountAsync();
            var lowStockProducts = await _context.Products
                .Where(p => p.StockCount <= p.MinStockLevel)
                .ToListAsync();

            var salesToday = await _context.Sales
                .Where(s => s.CreatedAt.Date == today && s.Status != "Cancelled")
                .ToListAsync();

            var salesThisMonth = await _context.Sales
                .Where(s => s.CreatedAt >= thisMonthStart && s.Status != "Cancelled")
                .ToListAsync();

            var salesLastMonth = await _context.Sales
                .Where(s => s.CreatedAt >= lastMonthStart && s.CreatedAt < thisMonthStart && s.Status != "Cancelled")
                .ToListAsync();

            var openServiceOrders = await _context.ServiceOrders
                .CountAsync(s => s.Status == "Beklemede" || s.Status == "Tamirde");

            var pendingTasks = await _context.ProjectTasks
                .CountAsync(t => t.Status == "Yapılacak" || t.Status == "Yapılıyor");

            // --- SON 5 SATIŞ ---
            var recentSalesRaw = await _context.Sales
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .ToListAsync();

            var recentSales = new List<RecentSaleDto>();
            foreach (var s in recentSalesRaw)
            {
                var product = await _context.Products.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.Id == s.ProductId);
                var customer = await _context.Customers.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.Id == s.CustomerId);

                recentSales.Add(new RecentSaleDto
                {
                    Id = s.Id,
                    ProductName = product?.Name ?? "-",
                    CustomerName = customer?.FullName ?? "-",
                    TotalAmount = s.TotalAmount,
                    CreatedAt = s.CreatedAt,
                    Status = s.Status
                });
            }

            // --- AYLIK SATIŞ GRAFİĞİ (Son 6 ay) ---
            var monthlySales = new List<MonthlySaleDto>();
            var monthNames = new[] { "", "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran",
                                      "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };

            for (int i = 5; i >= 0; i--)
            {
                var monthStart = thisMonthStart.AddMonths(-i);
                var monthEnd = monthStart.AddMonths(1);
                var monthlySalesData = await _context.Sales
                    .Where(s => s.CreatedAt >= monthStart && s.CreatedAt < monthEnd && s.Status != "Cancelled")
                    .ToListAsync();

                monthlySales.Add(new MonthlySaleDto
                {
                    Month = monthNames[monthStart.Month],
                    Year = monthStart.Year,
                    Revenue = monthlySalesData.Sum(s => s.TotalAmount),
                    SaleCount = monthlySalesData.Count
                });
            }

            // --- EN ÇOK SATILAN 5 ÜRÜN ---
            var topProductsRaw = await _context.Sales
                .Where(s => s.Status != "Cancelled")
                .GroupBy(s => s.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(s => s.Quantity),
                    TotalRevenue = g.Sum(s => s.TotalAmount)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(5)
                .ToListAsync();

            var topProducts = new List<TopProductDto>();
            foreach (var t in topProductsRaw)
            {
                var product = await _context.Products.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.Id == t.ProductId);
                topProducts.Add(new TopProductDto
                {
                    ProductName = product?.Name ?? "-",
                    TotalQuantity = t.TotalQuantity,
                    TotalRevenue = t.TotalRevenue
                });
            }

            return new DashboardDto
            {
                TotalProducts = totalProducts,
                TotalCustomers = totalCustomers,
                TotalSalesToday = salesToday.Count,
                TotalSalesThisMonth = salesThisMonth.Count,
                OpenServiceOrders = openServiceOrders,
                PendingTasks = pendingTasks,
                LowStockProductCount = lowStockProducts.Count,
                TodayRevenue = salesToday.Sum(s => s.TotalAmount),
                ThisMonthRevenue = salesThisMonth.Sum(s => s.TotalAmount),
                LastMonthRevenue = salesLastMonth.Sum(s => s.TotalAmount),
                RecentSales = recentSales,
                LowStockProducts = lowStockProducts.Select(p => new LowStockProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    StockCount = p.StockCount,
                    MinStockLevel = p.MinStockLevel
                }).ToList(),
                MonthlySales = monthlySales,
                TopProducts = topProducts
            };
        }
    }
}
