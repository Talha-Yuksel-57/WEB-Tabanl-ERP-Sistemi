using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.Core.Entities
{
    public class Product : BaseEntity
    {
        // Id ve TenantId BaseEntity'den gelir

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        // Stok miktarı
        public int StockCount { get; set; }

        // Kritik stok seviyesi
        public int MinStockLevel { get; set; }

        // Ürün aktif mi?
        public bool IsActive { get; set; } = true;

        // Concurrency Control
        // Aynı kaydı iki kişinin aynı anda güncellemesini kontrol eder
        [Timestamp]
        public byte[] RowVersion { get; set; }

        // İlişki
        public virtual Tenant? Tenant { get; set; }
    }
}