# Migration Notları

## Adım 2 — AppUser Güncelleme
AppUser'a RefreshToken alanları eklendi.

## Adım 3 — AuditLog Tablosu (YENİ)
AuditLogs tablosu eklendi.

## Tüm migration'ları tek seferde çalıştır:

Visual Studio Package Manager Console (ERP.Data projesi seçili):
```
Add-Migration AddRefreshTokenAndAuditLog
Update-Database
```

Terminal (ERP.API klasöründe):
```
dotnet ef migrations add AddRefreshTokenAndAuditLog --project ../ERP.Data
dotnet ef database update --project ../ERP.Data
```
