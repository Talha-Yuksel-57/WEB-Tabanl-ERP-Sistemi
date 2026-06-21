import { useEffect, useState } from "react";

// Bildirim türlerine (severity) göre arayüzde kullanılacak renk paletini tutan nesne (Style Dictionary)
// info, success, warning ve error durumları için arka plan, kenarlık ve yazı renkleri belirlenmiştir.
const SEVERITY_STYLE = {
  info: { bg: "#eff6ff", border: "#2563eb", color: "#1e3a8a" },
  success: { bg: "#f0fdf4", border: "#16a34a", color: "#14532d" },
  warning: { bg: "#fffbeb", border: "#d97706", color: "#78350f" },
  error: { bg: "#fef2f2", border: "#dc2626", color: "#7f1d1d" },
};

// Uygulama genelinde anlık bildirim kartlarını (Toast) ekranda listeleyen bileşen
// Parametre olarak aktif bildirim dizisini (notifications) ve bir bildirimi kapatma fonksiyonunu (onDismiss) alır.
export default function NotificationToasts({ notifications, onDismiss }) {
  // Eğer görüntülenecek aktif bir bildirim yoksa, DOM'a gereksiz eleman çizmemek için doğrudan null döner
  if (!notifications || notifications.length === 0) return null;

  return (
    <div
      // Tüm bildirim kartlarını ekranın sağ üst köşesinde sabit (fixed) tutan ve üst üste (flex-column) dizen kapsayıcı stil
      style={{
        position: "fixed",
        top: 16,
        right: 16,
        zIndex: 1000, // Bildirimlerin diğer tüm sayfa elemanlarının üstünde görünmesini garanti eder
        display: "flex",
        flexDirection: "column",
        gap: 8, // Bildirim kartlarının arasındaki boşluk
        maxWidth: 340,
      }}
    >
      {/* Aktif bildirimler dizisi döngüye alınarak her biri ekrana birer kart olarak basılır */}
      {notifications.map((n) => {
        // Gelen bildirimin önem derecesine uygun rengi seçer, bilinmeyen bir türse varsayılan olarak 'info' stilini uygular
        const style = SEVERITY_STYLE[n.severity] || SEVERITY_STYLE.info;
        
        return (
          <div
            key={n.id} // React'in render performansını optimize etmesi için benzersiz anahtar (ID) değeri
            style={{
              background: style.bg,
              border: `1px solid ${style.border}`,
              color: style.color,
              borderRadius: 8,
              padding: "10px 12px",
              boxShadow: "0 4px 12px rgba(0,0,0,0.08)", // Karta hafif derinlik kazandıran gölge efekti
              cursor: "pointer",
            }}
            // Kullanıcı bildirim kartına tıkladığında, o bildirimi listeden kaldıran (kapatan) üst fonksiyonu tetikler
            onClick={() => onDismiss(n.id)}
            title="Kapatmak için tıkla"
          >
            {/* Bildirim Başlığı (Örn: "Stok Uyarısı", "İşlem Başarılı") */}
            <div style={{ fontWeight: 700, fontSize: 13 }}>{n.title}</div>
            
            {/* Bildirim İçerik Mesajı (Örn: "X ürününün stoğu kritik seviyenin altına düştü!") */}
            <div style={{ fontSize: 13, marginTop: 2 }}>{n.message}</div>
          </div>
        );
      })}
    </div>
  );
}