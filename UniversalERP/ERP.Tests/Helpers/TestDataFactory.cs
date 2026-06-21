using ERP.Core.Entities;

namespace ERP.Tests.Helpers
{
    /// <summary>
    /// Testlerde tekrar tekrar kullanılan örnek veri oluşturucular.
    /// </summary>
    public static class TestDataFactory
    {
        public static Product CreateProduct(
            int id = 1,
            string name = "Test Ürünü",
            decimal price = 100m,
            int stock = 50,
            int minStock = 5,
            int tenantId = 1)
        {
            return new Product
            {
                Id = id,
                Name = name,
                Price = price,
                StockCount = stock,
                MinStockLevel = minStock,
                IsActive = true,
                IsDeleted = false,
                TenantId = tenantId,
                CreatedAt = DateTime.Now
            };
        }

        public static Customer CreateCustomer(
            int id = 1,
            string fullName = "Test Müşterisi",
            string email = "test@test.com",
            int tenantId = 1)
        {
            return new Customer
            {
                Id = id,
                FullName = fullName,
                Email = email,
                Phone = "05001234567",
                IsDeleted = false,
                TenantId = tenantId,
                CreatedAt = DateTime.Now
            };
        }

        public static Sale CreateSale(
            int id = 1,
            int productId = 1,
            int customerId = 1,
            int quantity = 2,
            decimal totalAmount = 200m,
            string status = "Completed",
            int tenantId = 1)
        {
            return new Sale
            {
                Id = id,
                ProductId = productId,
                CustomerId = customerId,
                Quantity = quantity,
                TotalAmount = totalAmount,
                PaymentMethod = "Nakit",
                Status = status,
                CashierId = 1,
                TenantId = tenantId,
                CreatedAt = DateTime.Now
            };
        }
    }
}
