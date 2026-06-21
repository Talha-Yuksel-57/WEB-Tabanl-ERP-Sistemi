namespace ERP.Core.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int TenantId { get; set; }

        // Kim yaptı
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;

        // Ne yaptı
        public string Action { get; set; } = string.Empty;      // Create, Update, Delete
        public string EntityName { get; set; } = string.Empty;  // Product, Sale, Customer...
        public string EntityId { get; set; } = string.Empty;    // Hangi kaydın ID'si

        // Ne değişti
        public string? OldValues { get; set; }   // JSON — önceki değerler
        public string? NewValues { get; set; }   // JSON — yeni değerler

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? IpAddress { get; set; }
    }
}
