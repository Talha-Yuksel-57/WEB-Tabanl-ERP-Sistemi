using System;

namespace ERP.Core.Entities
{
    public abstract class BaseEntity
    {
        public int Id { get; set; } // Birincil Anahtar
        public int TenantId { get; set; } // Çok kiracılı yapı bağlacı
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false; // Yumuşak silme kontrolü
    }
}