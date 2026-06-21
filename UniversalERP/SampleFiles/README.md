# Import Örnek Dosyaları

Bu klasördeki dosyaları `POST /api/reports/products/import/*` endpoint'lerine yükleyerek test edebilirsin.

## Excel (.xlsx) Formatı

Excel dosyasında **1. satır başlık**, 2. satırdan itibaren veri olmalı:

| A (Ad)              | B (Fiyat) | C (Stok) | D (MinStok) |
|---------------------|-----------|----------|-------------|
| Samsung Galaxy S24  | 45000     | 20       | 3           |
| iPhone 15 Pro       | 65000     | 15       | 3           |

## JSON Formatı (urunler-ornek.json)

```json
[
  {
    "name": "Ürün Adı",
    "price": 100.00,
    "stockCount": 50,
    "minStockLevel": 5
  }
]
```

## XML Formatı (urunler-ornek.xml)

```xml
<?xml version="1.0" encoding="utf-8"?>
<Products>
  <ProductImportRowDto>
    <Name>Ürün Adı</Name>
    <Price>100.00</Price>
    <StockCount>50</StockCount>
    <MinStockLevel>5</MinStockLevel>
  </ProductImportRowDto>
</Products>
```

## Export Endpoint'leri

| Endpoint                          | Açıklama                    |
|-----------------------------------|-----------------------------|
| GET /api/reports/sales/pdf        | Satış raporu PDF indir      |
| GET /api/reports/sales/excel      | Satış raporu Excel indir    |
| GET /api/reports/stock/pdf        | Stok raporu PDF indir       |
| GET /api/reports/stock/excel      | Stok raporu Excel indir     |
| GET /api/reports/sales/preview    | Satış verilerini JSON gör   |
| GET /api/reports/stock/preview    | Stok verilerini JSON gör    |

### Filtre parametreleri (satış raporları için):
- `startDate` — Başlangıç tarihi (ör: 2026-01-01)
- `endDate` — Bitiş tarihi
- `status` — Completed veya Cancelled
- `paymentMethod` — Nakit, Kredi Kartı, Havale
