namespace ERP.Core.DTOs.Product
{
    // Ürün listeleme ve detay için
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int StockCount { get; set; }
        public int MinStockLevel { get; set; }
        public bool IsActive { get; set; }
        public bool IsLowStock => StockCount <= MinStockLevel;
        public DateTime CreatedAt { get; set; }
    }

    // Yeni ürün oluştururken kullanılır
    public class CreateProductDto
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int StockCount { get; set; }
        public int MinStockLevel { get; set; } = 5;
    }

    // Ürün güncellerken kullanılır
    public class UpdateProductDto
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int MinStockLevel { get; set; }
        public bool IsActive { get; set; }
    }

    // Stok güncelleme için özel DTO
    public class UpdateStockDto
    {
        public int NewStock { get; set; }
    }
}
