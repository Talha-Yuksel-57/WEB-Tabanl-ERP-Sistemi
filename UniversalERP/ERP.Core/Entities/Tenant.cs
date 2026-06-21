using System;
using System.Collections.Generic;

namespace ERP.Core.Entities
{
    public class Tenant : BaseEntity
    {
        // Id, TenantId ve CreatedAt alanları BaseEntity'den otomatik gelir

        public string Name { get; set; } // Diyagramdaki 'Name' alanı
        public string TaxNumber { get; set; } // Vergi Numarası
        public string Address { get; set; } // Firma Adresi
        public string Industry { get; set; } // Sektör Bilgisi

        // --- ABONELİK VE DURUM YÖNETİMİ ---
        public bool IsActive { get; set; } = true; // Firmayı aktif/pasif yapmak için
        public string PlanType { get; set; } = "Free"; // Free, Gold, Enterprise
        public DateTime? SubscriptionEndDate { get; set; } // Lisans bitiş tarihi

        // --- GÜVENLİK VE İLİŞKİLER ---
        public string TenantGuid { get; set; } = Guid.NewGuid().ToString(); // Benzersiz kimlik

        // Bir firmanın birden fazla kullanıcısı olabilir
        public virtual ICollection<AppUser> Users { get; set; }
    }
}