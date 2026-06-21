using Microsoft.AspNetCore.Identity;
using System;

namespace ERP.Core.Entities
{
    public class AppUser : IdentityUser
    {
        // IdentityUser zaten bir 'Id' (string) içerir, ancak ERP içindeki 
        // tüm tablolarla uyum için FullName ve TenantId'yi yönetmeliyiz

        public string FullName { get; set; }

        // Multi-tenant yapısı için zorunlu alan
        public int TenantId { get; set; }
        public virtual Tenant? Tenant { get; set; }

        // --- PROFESYONEL ERP VE SENARYO ALANLARI ---

        // Teknisyen ve Yazılımcı maliyet hesabı için
        public decimal Salary { get; set; }

        // Personelin hangi birimde olduğu (Yazılım, Teknik Servis, Satış)
        public string? Department { get; set; }

        // İşe giriş tarihi
        public DateTime HireDate { get; set; } = DateTime.Now;

        // BaseEntity miras alınamadığı (IdentityUser nedeniyle) için ortak alanları manuel ekliyoruz
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false;

        public string Role { get; set; }

        public bool IsActive { get; set; } = true;

        // Refresh Token — logout'ta null'lanır, token yenilemede güncellenir
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
    }
}