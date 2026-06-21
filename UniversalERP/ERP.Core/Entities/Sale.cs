using System;
using System.Collections.Generic;

namespace ERP.Core.Entities
{
    public class Sale : BaseEntity
    {
        // Id, TenantId ve CreatedAt (SaleDate yerine) BaseEntity'den gelir

        public decimal TotalAmount { get; set; } // Toplam Satış Tutarı

        // Ödeme Yöntemi: Nakit, Kredi Kartı
        public string PaymentMethod { get; set; }

        // Satışı yapan personel (Kasiyer)
        public int CashierId { get; set; }

        // Müşteri İlişkisi: Fatura kime kesildi?
        public int CustomerId { get; set; }

        // Durum: Tamamlandı, İptal Edildi, İade
        public string Status { get; set; } = "Completed";

        // --- STOK VE ÜRÜN BAĞLANTISI ---
        // Bir satışta birden fazla ürün olabilir (Opsiyonel ama profesyonel yaklaşım)
        // Eğer tek ürün satılıyorsa ProductId olarak basitleştirilebilir.
        public int ProductId { get; set; }
        public int Quantity { get; set; } // Satılan miktar
    }
}