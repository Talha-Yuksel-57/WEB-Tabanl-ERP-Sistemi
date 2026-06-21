# UniversalERP — Multi-Tenant ERP Sistemi

Kurumsal seviyede, çok kiracılı (multi-tenant) SaaS ERP sistemi.

---

## Mimari

```
ERP.Core       → Entity'ler, Interface'ler, DTO'lar, AutoMapper profili
ERP.Data       → Repository, UnitOfWork, Service implementasyonları, DbContext
ERP.API        → Controller'lar, Middleware, Validator'lar, SignalR Hub
ERP.Tests      → Unit testler (xUnit + Moq + FluentAssertions)
```

## Teknoloji Yığını

| Katman | Teknoloji |
|--------|-----------|
| Framework | ASP.NET Core 8 |
| ORM | Entity Framework Core 8 |
| Veritabanı | SQL Server (LocalDB geliştirme) |
| Kimlik Doğrulama | ASP.NET Core Identity + JWT |
| Mapping | AutoMapper 13 |
| Validasyon | FluentValidation 11 |
| Loglama | Serilog (Console + File) |
| Gerçek Zamanlı | SignalR |
| PDF | QuestPDF |
| Excel | ClosedXML |
| Test | xUnit + Moq + FluentAssertions |
| Dokümantasyon | Swagger / OpenAPI |

---

## Kurulum

### 1. Gereksinimler
- .NET 8 SDK
- SQL Server veya LocalDB

### 2. Veritabanını oluştur

```bash
cd ERP.API
dotnet ef migrations add InitialCreate --project ../ERP.Data
dotnet ef database update --project ../ERP.Data
```

### 3. API'yi başlat

```bash
cd ERP.API
dotnet run
```

Swagger UI: `https://localhost:{port}/swagger`

### 4. Test kullanıcısı
- **Email:** `master@erp.com`
- **Şifre:** `Master123!`
- **Rol:** SuperAdmin

---

## Güvenlik Özellikleri

| Özellik | Açıklama |
|---------|----------|
| JWT Authentication | Access Token (60 dk) + Refresh Token (7 gün) |
| Role-Based Authorization | SuperAdmin / TenantAdmin / Manager / Employee / Cashier / Technician |
| Multi-Tenant Isolation | Her firma sadece kendi verisini görür (Global Query Filter) |
| Soft Delete | Kayıtlar fiziksel silinmez, IsDeleted=true yapılır |
| Optimistic Concurrency | RowVersion ile eş zamanlı güncelleme koruması |
| Audit Log | Kim, ne zaman, neyi değiştirdi kaydı |
| ACID Transaction | Satış işlemi atomik: stok + satış kaydı birlikte başarılı olur veya ikisi de geri alınır |
| Password Hash | ASP.NET Identity PBKDF2 ile hash |
| HTTPS | Zorunlu yönlendirme |

---

## API Endpoint'leri

### Kimlik Doğrulama
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| POST | `/api/auth/register` | Yeni kullanıcı kaydı |
| POST | `/api/auth/login` | Giriş yap → AccessToken + RefreshToken |
| POST | `/api/auth/refresh` | Token yenile |
| POST | `/api/auth/logout` | Çıkış yap |

### Ürünler
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/products` | Tüm ürünler |
| GET | `/api/products/{id}` | Ürün detayı |
| GET | `/api/products/low-stock` | Kritik stok uyarısı |
| POST | `/api/products` | Yeni ürün |
| PUT | `/api/products/{id}` | Ürün güncelle |
| PATCH | `/api/products/{id}/stock` | Stok güncelle |
| DELETE | `/api/products/{id}` | Soft delete |

### Satışlar (ACID Transaction)
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/sales` | Tüm satışlar |
| POST | `/api/sales` | Satış yap (stok otomatik düşer) |
| POST | `/api/sales/{id}/cancel` | İptal et (stok iade edilir) |

### Raporlar
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/reports/sales/pdf` | Satış raporu PDF |
| GET | `/api/reports/sales/excel` | Satış raporu Excel |
| GET | `/api/reports/stock/pdf` | Stok raporu PDF |
| GET | `/api/reports/stock/excel` | Stok raporu Excel |
| POST | `/api/reports/products/import/excel` | Excel'den ürün aktar |
| POST | `/api/reports/products/import/json` | JSON'dan ürün aktar |
| POST | `/api/reports/products/import/xml` | XML'den ürün aktar |

### Dashboard
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/dashboard` | KPI'lar + grafikler + son işlemler |

### Audit Log
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/auditlogs` | İşlem geçmişi (filtre + sayfalama) |

---

## SignalR

Hub URL: `/hubs/erp`

### Frontend bağlantısı (JavaScript):
```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/erp", { accessTokenFactory: () => localStorage.getItem("token") })
  .withAutomaticReconnect()
  .build();

connection.on("ReceiveNotification", (notification) => {
  console.log(notification.type, notification.message);
  // { type, title, message, severity, data, timestamp }
});

await connection.start();
```

### Bildirim tipleri:
| Type | Tetikleyen | Severity |
|------|-----------|----------|
| `NewSale` | Satış yapıldığında | success |
| `LowStock` | Stok kritik seviyeye düştüğünde | warning |
| `NewServiceOrder` | Servis talebi açıldığında | info |

---

## Testleri Çalıştır

```bash
cd ERP.Tests
dotnet test
```

Veya Visual Studio'da Test Explorer'dan çalıştır.

### Test kapsamı:
- `ProductServiceTests` — 10 test (CRUD + soft delete + low stock)
- `SaleServiceTests` — 9 test (ACID transaction + stok düşme + iptal + hesaplama)
- `CustomerServiceTests` — 4 test
- `ValidatorTests` — 15 test (Product + Customer + Sale validasyonları)

---

## Log Dosyaları

```
ERP.API/Logs/erp-YYYY-MM-DD.log
```

Her gün yeni dosya oluşturulur. Login, logout, CRUD işlemleri ve hatalar kaydedilir.
