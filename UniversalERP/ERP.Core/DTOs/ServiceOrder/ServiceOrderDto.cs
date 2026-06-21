namespace ERP.Core.DTOs.ServiceOrder
{
    public class ServiceOrderDto
    {
        public int Id { get; set; }
        public string DeviceName { get; set; }
        public string CustomerName { get; set; }
        public int CustomerId { get; set; }
        public string IssueDescription { get; set; }
        public string Status { get; set; }
        public decimal ServiceFee { get; set; }
        public int? AssignedUserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateServiceOrderDto
    {
        public string DeviceName { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string IssueDescription { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal PartCost { get; set; }
        public int? AssignedUserId { get; set; }
    }

    public class UpdateServiceOrderStatusDto
    {
        public string Status { get; set; } // Beklemede, Tamirde, Tamamlandı, Teslim Edildi
    }
}
