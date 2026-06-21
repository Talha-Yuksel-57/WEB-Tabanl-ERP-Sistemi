# UniversalERP Frontend

React + Vite tabanlı frontend. UniversalERP backend API'sine bağlanır.

## Kurulum

```bash
npm install
npm run dev
```

Tarayıcıda `http://localhost:5173` açılır.

## Backend Bağlantısı

`.env` dosyasında backend adresi tanımlı:

```
VITE_API_BASE_URL=http://localhost:5240
```

## Test Kullanıcısı

- Email: `master@erp.com`
- Şifre: `Master123!`

## Tüm Sayfalar

- **Login / Register** — JWT ile giriş, kayıt formunda firma seçimi
- **Dashboard** — KPI kartları, son satışlar, kritik stok, en çok satılan ürünler
- **Ürünler** — CRUD, stok satır içi düzenleme, kritik stok vurgusu
- **Müşteriler** — CRUD
- **Satış** — ürün/müşteri seçerek satış (ACID transaction), iptal
- **Servis** — teknik servis kaydı, durum takibi (renk kodlu)
- **Görevler** — proje görev yönetimi, öncelik/durum
- **Raporlar** — PDF/Excel export (satış, stok), Excel/JSON/XML import
- **İşlem Geçmişi** — audit log görüntüleme, filtreleme (sadece Admin)
- **Profil** — ad/departman güncelleme, şifre değiştirme
- **Firma Ayarları** — firma bilgileri güncelleme (sadece Admin)

## Özellikler

- Role bazlı menü ve buton görünürlüğü (backend `[Authorize(Roles=...)]` ile birebir eşleşir)
- SignalR ile gerçek zamanlı bildirimler (stok uyarısı, yeni satış)
- Açık/Koyu tema (tercih localStorage'da saklanır)
- Responsive tasarım (mobil/tablet için optimize menü ve form düzeni)

## Mimari Notları

- `src/api/http.js` — fetch wrapper, Bearer token otomatik ekleniyor
- `src/api/signalr.js` — Hub bağlantısı, otomatik yeniden bağlanma
- `src/utils/permissions.js` — backend rol kısıtlarıyla birebir eşleşen liste
- `src/components/Layout.jsx` — role bazlı menü + tema toggle
