using System;

namespace ERP.Core.Entities
{
    public class ServiceOrder : BaseEntity
    {
        // Id ve TenantId BaseEntity'den miras alınır

        public string DeviceName { get; set; } // Örn: iPhone 13

        // Müşteri ilişkisi: Diyagramda Customers tablosuna bağlıdır
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } // Hızlı erişim için tutulabilir

        public string IssueDescription { get; set; } // Arıza: Ekran kırık

        // Durum yönetimi: Sınıf diyagramındaki UpdateJobStatus metodu burayı günceller
        public string Status { get; set; } // Beklemede, Tamirde, Tamamlandı, Teslim Edildi

        // Finansal Veriler
        public decimal ServiceFee { get; set; } // Müşteriye yansıtılan toplam işçilik + parça ücreti

        // İç Maliyet: Sınıf diyagramındaki CalculateJobCost metodu için gereklidir
        // Not: Bu alan yetkilendirme ile teknisyenden gizlenecek, sadece yönetici görecek.
        public decimal PartCost { get; set; }

        // Personel Ataması: User tablosuna bağlıdır
        public int? AssignedUserId { get; set; }

        // İşin hangi proje kapsamında olduğu (Yazılımcı senaryosu için opsiyonel)
        public int? ProjectId { get; set; }
    }
}