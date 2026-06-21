using System;

namespace ERP.Core.Entities
{
    public class Customer : BaseEntity
    {
        // Id ve TenantId alanları BaseEntity'den miras alınır

        // E-R ve Sınıf diyagramıyla uyumlu olması için FullName kullanıyoruz
        public string FullName { get; set; }

        public string? Email { get; set; }

        // Diyagramdaki 'Phone' alanıyla uyum için isimlendirme güncellendi
        public string? Phone { get; set; }

        public string? Address { get; set; }

        // İlişkisel özellik (Navigation Property)
        public virtual Tenant? Tenant { get; set; }
    }
}