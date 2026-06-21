namespace ERP.Core.DTOs.Sale
{
    public class SaleDto
    {
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Satış oluştururken kullanılır — ACID transaction tetikler
    public class CreateSaleDto
    {
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public int Quantity { get; set; }
        public string PaymentMethod { get; set; } = "Nakit";
    }
}
