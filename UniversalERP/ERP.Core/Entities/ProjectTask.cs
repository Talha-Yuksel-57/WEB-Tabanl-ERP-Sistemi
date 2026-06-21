using System;

namespace ERP.Core.Entities
{
    public class ProjectTask : BaseEntity
    {
        // Id ve TenantId BaseEntity'den miras alınır

        public string TaskTitle { get; set; } // Görev Başlığı
        public string Description { get; set; } // Görev Detayı

        // Zaman Takibi: Sınıf diyagramındaki LogWorkHours metodu burayı günceller
        public int HoursWorked { get; set; }

        // Görev Durumu: Yapılacak, Yapılıyor, Bitti
        public string Status { get; set; }

        // Atanan Yazılımcı: Users tablosuna bağlıdır
        // Sınıf diyagramındaki 'assigned' ilişkisi için DeveloperId (int) olarak güncellendi.
        public int? AssignedDeveloperId { get; set; }

        // Proje Bağlantısı: Bu görev hangi projeye ait?
        public int ProjectId { get; set; }

        // Öncelik: Düşük, Orta, Yüksek (Profesyonel diyagramda yer alan ek bir alan)
        public string Priority { get; set; } = "Medium";
    }
}