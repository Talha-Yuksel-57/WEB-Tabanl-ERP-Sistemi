using ClosedXML.Excel;
using ERP.Core.DTOs.Report;
using ERP.Core.Entities;
using ERP.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.Json;
using System.Xml.Serialization;

namespace ERP.Data.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public ReportService(AppDbContext context, IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // ─────────────────────────────────────────────────────────
        // VERİ HAZIRLAMA
        // ─────────────────────────────────────────────────────────

        public async Task<List<SaleReportRowDto>> GetSaleReportDataAsync(SaleReportFilterDto filter)
        {
            var query = _context.Sales.AsQueryable();

            if (filter.StartDate.HasValue)
                query = query.Where(s => s.CreatedAt >= filter.StartDate.Value);
            if (filter.EndDate.HasValue)
                query = query.Where(s => s.CreatedAt <= filter.EndDate.Value.AddDays(1));
            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(s => s.Status == filter.Status);
            if (!string.IsNullOrEmpty(filter.PaymentMethod))
                query = query.Where(s => s.PaymentMethod == filter.PaymentMethod);

            var sales = await query.OrderByDescending(s => s.CreatedAt).ToListAsync();

            var rows = new List<SaleReportRowDto>();
            foreach (var s in sales)
            {
                var product = await _context.Products.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.Id == s.ProductId);
                var customer = await _context.Customers.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.Id == s.CustomerId);

                rows.Add(new SaleReportRowDto
                {
                    Id = s.Id,
                    CustomerName = customer?.FullName ?? "-",
                    ProductName = product?.Name ?? "-",
                    Quantity = s.Quantity,
                    TotalAmount = s.TotalAmount,
                    PaymentMethod = s.PaymentMethod,
                    Status = s.Status,
                    SaleDate = s.CreatedAt
                });
            }
            return rows;
        }

        public async Task<List<StockReportRowDto>> GetStockReportDataAsync()
        {
            var products = await _context.Products.OrderBy(p => p.Name).ToListAsync();
            return products.Select(p => new StockReportRowDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                StockCount = p.StockCount,
                MinStockLevel = p.MinStockLevel,
                IsLowStock = p.StockCount <= p.MinStockLevel,
                IsActive = p.IsActive
            }).ToList();
        }

        // ─────────────────────────────────────────────────────────
        // PDF EXPORT
        // ─────────────────────────────────────────────────────────

        public async Task<byte[]> ExportSalesToPdfAsync(SaleReportFilterDto filter)
        {
            var rows = await GetSaleReportDataAsync(filter);
            var totalRevenue = rows.Where(r => r.Status == "Completed").Sum(r => r.TotalAmount);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("UniversalERP — Satış Raporu")
                            .FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text($"Oluşturulma: {DateTime.Now:dd.MM.yyyy HH:mm}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().PaddingTop(10).Column(col =>
                    {
                        // Özet satırı
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(8).Column(c =>
                            {
                                c.Item().Text("Toplam Satış").FontSize(9).FontColor(Colors.Grey.Darken2);
                                c.Item().Text(rows.Count.ToString()).FontSize(16).Bold();
                            });
                            row.ConstantItem(10);
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(8).Column(c =>
                            {
                                c.Item().Text("Toplam Gelir").FontSize(9).FontColor(Colors.Grey.Darken2);
                                c.Item().Text($"{totalRevenue:N2} TL").FontSize(16).Bold().FontColor(Colors.Green.Darken2);
                            });
                            row.ConstantItem(10);
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(8).Column(c =>
                            {
                                c.Item().Text("İptal Edilen").FontSize(9).FontColor(Colors.Grey.Darken2);
                                c.Item().Text(rows.Count(r => r.Status == "Cancelled").ToString())
                                    .FontSize(16).Bold().FontColor(Colors.Red.Darken2);
                            });
                        });

                        col.Item().PaddingTop(12);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(35);
                                cols.RelativeColumn(2.5f);
                                cols.RelativeColumn(2.5f);
                                cols.ConstantColumn(50);
                                cols.RelativeColumn(1.5f);
                                cols.RelativeColumn(1.5f);
                                cols.ConstantColumn(70);
                                cols.RelativeColumn(1.5f);
                            });

                            static IContainer HeaderCell(IContainer c) =>
                                c.Background(Colors.Blue.Darken2).Padding(5);

                            table.Header(header =>
                            {
                                foreach (var h in new[] { "#", "Müşteri", "Ürün", "Adet", "Tutar", "Ödeme", "Durum", "Tarih" })
                                    header.Cell().Element(HeaderCell)
                                        .Text(h).FontColor(Colors.White).Bold().FontSize(9);
                            });

                            bool isGray = false;
                            foreach (var r in rows)
                            {
                                var bg = isGray ? Colors.Grey.Lighten4 : Colors.White;
                                isGray = !isGray;

                                IContainer DataCell(IContainer c) =>
                                    c.Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4);

                                table.Cell().Element(DataCell).Text(r.Id.ToString()).FontSize(9);
                                table.Cell().Element(DataCell).Text(r.CustomerName).FontSize(9);
                                table.Cell().Element(DataCell).Text(r.ProductName).FontSize(9);
                                table.Cell().Element(DataCell).Text(r.Quantity.ToString()).FontSize(9);
                                table.Cell().Element(DataCell).Text($"{r.TotalAmount:N2} TL").FontSize(9);
                                table.Cell().Element(DataCell).Text(r.PaymentMethod).FontSize(9);
                                table.Cell().Element(DataCell)
                                    .Text(r.Status == "Completed" ? "Tamamlandi" : "Iptal")
                                    .FontColor(r.Status == "Completed" ? Colors.Green.Darken2 : Colors.Red.Darken2)
                                    .FontSize(9);
                                table.Cell().Element(DataCell).Text(r.SaleDate.ToString("dd.MM.yy HH:mm")).FontSize(9);
                            }
                        });
                    });

                    // FOOTER — void hatası: Text() sonrası zincir kurulamaz, ayrı yazıyoruz
                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.Span("UniversalERP | Sayfa ").FontSize(8).FontColor(Colors.Grey.Darken1);
                        t.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                        t.Span(" / ").FontSize(8).FontColor(Colors.Grey.Darken1);
                        t.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> ExportStockToPdfAsync()
        {
            var rows = await GetStockReportDataAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("UniversalERP — Stok Raporu")
                            .FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text($"Olusturulma: {DateTime.Now:dd.MM.yyyy HH:mm}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(35);
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(1.5f);
                            cols.RelativeColumn(1.5f);
                            cols.RelativeColumn(1.5f);
                            cols.RelativeColumn(1.5f);
                        });

                        static IContainer HeaderCell(IContainer c) =>
                            c.Background(Colors.Blue.Darken2).Padding(5);

                        table.Header(header =>
                        {
                            foreach (var h in new[] { "#", "Urun Adi", "Fiyat", "Stok", "Min.Stok", "Durum" })
                                header.Cell().Element(HeaderCell)
                                    .Text(h).FontColor(Colors.White).Bold().FontSize(9);
                        });

                        bool isGray = false;
                        foreach (var r in rows)
                        {
                            var bg = r.IsLowStock ? Colors.Red.Lighten4 : (isGray ? Colors.Grey.Lighten4 : Colors.White);
                            isGray = !isGray;

                            IContainer DataCell(IContainer c) =>
                                c.Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4);

                            table.Cell().Element(DataCell).Text(r.Id.ToString()).FontSize(9);
                            table.Cell().Element(DataCell).Text(r.Name).FontSize(9);
                            table.Cell().Element(DataCell).Text($"{r.Price:N2} TL").FontSize(9);

                            // Bold(bool) yok — if ile ayırıyoruz
                            if (r.IsLowStock)
                                table.Cell().Element(DataCell).Text(r.StockCount.ToString())
                                    .FontColor(Colors.Red.Darken2).Bold().FontSize(9);
                            else
                                table.Cell().Element(DataCell).Text(r.StockCount.ToString()).FontSize(9);

                            table.Cell().Element(DataCell).Text(r.MinStockLevel.ToString()).FontSize(9);
                            table.Cell().Element(DataCell)
                                .Text(r.IsLowStock ? "Kritik" : (r.IsActive ? "Normal" : "Pasif"))
                                .FontColor(r.IsLowStock ? Colors.Red.Darken2 : Colors.Green.Darken2).FontSize(9);
                        }
                    });

                    // FOOTER — aynı düzeltme
                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.Span("UniversalERP | Sayfa ").FontSize(8).FontColor(Colors.Grey.Darken1);
                        t.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                        t.Span(" / ").FontSize(8).FontColor(Colors.Grey.Darken1);
                        t.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                });
            });

            return document.GeneratePdf();
        }

        // ─────────────────────────────────────────────────────────
        // EXCEL EXPORT
        // ─────────────────────────────────────────────────────────

        public async Task<byte[]> ExportSalesToExcelAsync(SaleReportFilterDto filter)
        {
            var rows = await GetSaleReportDataAsync(filter);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Satis Raporu");

            ws.Cell("A1").Value = "UniversalERP - Satis Raporu";
            ws.Cell("A1").Style.Font.FontSize = 16;
            ws.Cell("A1").Style.Font.Bold = true;
            ws.Cell("A2").Value = $"Olusturulma: {DateTime.Now:dd.MM.yyyy HH:mm}";

            var headers = new[] { "#", "Musteri", "Urun", "Adet", "Tutar (TL)", "Odeme Yontemi", "Durum", "Tarih" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(4, i + 1);
                cell.Value = headers[i];
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Font.Bold = true;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            int row = 5;
            foreach (var r in rows)
            {
                ws.Cell(row, 1).Value = r.Id;
                ws.Cell(row, 2).Value = r.CustomerName;
                ws.Cell(row, 3).Value = r.ProductName;
                ws.Cell(row, 4).Value = r.Quantity;
                ws.Cell(row, 5).Value = r.TotalAmount;
                ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 6).Value = r.PaymentMethod;
                ws.Cell(row, 7).Value = r.Status == "Completed" ? "Tamamlandi" : "Iptal";
                ws.Cell(row, 8).Value = r.SaleDate.ToString("dd.MM.yyyy HH:mm");

                if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#f5f7fa");

                ws.Cell(row, 7).Style.Font.FontColor =
                    r.Status == "Cancelled" ? XLColor.Red : XLColor.DarkGreen;

                row++;
            }

            ws.Cell(row, 4).Value = "TOPLAM:";
            ws.Cell(row, 4).Style.Font.Bold = true;
            ws.Cell(row, 5).FormulaA1 = $"=SUM(E5:E{row - 1})";
            ws.Cell(row, 5).Style.Font.Bold = true;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportStockToExcelAsync()
        {
            var rows = await GetStockReportDataAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Stok Raporu");

            ws.Cell("A1").Value = "UniversalERP - Stok Raporu";
            ws.Cell("A1").Style.Font.FontSize = 16;
            ws.Cell("A1").Style.Font.Bold = true;

            var headers = new[] { "#", "Urun Adi", "Fiyat (TL)", "Stok", "Min. Stok", "Kritik?", "Aktif?" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(4, i + 1);
                cell.Value = headers[i];
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Font.Bold = true;
            }

            int row = 5;
            foreach (var r in rows)
            {
                ws.Cell(row, 1).Value = r.Id;
                ws.Cell(row, 2).Value = r.Name;
                ws.Cell(row, 3).Value = r.Price;
                ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 4).Value = r.StockCount;
                ws.Cell(row, 5).Value = r.MinStockLevel;
                ws.Cell(row, 6).Value = r.IsLowStock ? "Kritik" : "Normal";
                ws.Cell(row, 7).Value = r.IsActive ? "Evet" : "Hayir";

                if (r.IsLowStock)
                {
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#ffe8e8");
                    ws.Cell(row, 4).Style.Font.FontColor = XLColor.Red;
                    ws.Cell(row, 4).Style.Font.Bold = true;
                }
                else if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#f5f7fa");

                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        // ─────────────────────────────────────────────────────────
        // IMPORT
        // ─────────────────────────────────────────────────────────

        public async Task<ImportResultDto> ImportProductsFromExcelAsync(Stream fileStream)
        {
            var result = new ImportResultDto();

            using var workbook = new XLWorkbook(fileStream);
            var ws = workbook.Worksheets.First();
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            result.TotalRows = Math.Max(0, lastRow - 1);

            for (int row = 2; row <= lastRow; row++)
            {
                try
                {
                    var name = ws.Cell(row, 1).GetString().Trim();
                    if (string.IsNullOrEmpty(name)) { result.Skipped++; continue; }

                    var priceStr = ws.Cell(row, 2).GetString();
                    if (!decimal.TryParse(priceStr, out var price) || price <= 0)
                    {
                        result.Errors.Add($"Satir {row}: Gecersiz fiyat '{priceStr}'");
                        result.Skipped++; continue;
                    }

                    int.TryParse(ws.Cell(row, 3).GetString(), out var stock);
                    int.TryParse(ws.Cell(row, 4).GetString(), out var minStock);

                    await _unitOfWork.Products.AddAsync(new Product
                    {
                        Name = name, Price = price, StockCount = stock,
                        MinStockLevel = minStock > 0 ? minStock : 5, IsActive = true
                    });
                    result.Imported++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Satir {row}: {ex.Message}");
                    result.Skipped++;
                }
            }

            if (result.Imported > 0) await _unitOfWork.CompleteAsync();
            return result;
        }

        public async Task<ImportResultDto> ImportProductsFromJsonAsync(Stream fileStream)
        {
            var result = new ImportResultDto();
            List<ProductImportRowDto>? rows;

            try
            {
                rows = await JsonSerializer.DeserializeAsync<List<ProductImportRowDto>>(
                    fileStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                result.Errors.Add($"JSON parse hatasi: {ex.Message}");
                return result;
            }

            if (rows == null || rows.Count == 0)
            {
                result.Errors.Add("Dosya bos veya gecersiz format.");
                return result;
            }

            result.TotalRows = rows.Count;

            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.Name)) { result.Skipped++; continue; }
                if (row.Price <= 0)
                {
                    result.Errors.Add($"'{row.Name}': Fiyat 0'dan buyuk olmalidir.");
                    result.Skipped++; continue;
                }

                await _unitOfWork.Products.AddAsync(new Product
                {
                    Name = row.Name, Price = row.Price, StockCount = row.StockCount,
                    MinStockLevel = row.MinStockLevel > 0 ? row.MinStockLevel : 5, IsActive = true
                });
                result.Imported++;
            }

            if (result.Imported > 0) await _unitOfWork.CompleteAsync();
            return result;
        }

        public async Task<ImportResultDto> ImportProductsFromXmlAsync(Stream fileStream)
        {
            var result = new ImportResultDto();
            List<ProductImportRowDto>? rows;

            try
            {
                var serializer = new XmlSerializer(typeof(List<ProductImportRowDto>),
                    new XmlRootAttribute("Products"));
                rows = (List<ProductImportRowDto>?)serializer.Deserialize(fileStream);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"XML parse hatasi: {ex.Message}");
                return result;
            }

            if (rows == null || rows.Count == 0)
            {
                result.Errors.Add("Dosya bos veya gecersiz format.");
                return result;
            }

            result.TotalRows = rows.Count;

            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.Name)) { result.Skipped++; continue; }
                if (row.Price <= 0)
                {
                    result.Errors.Add($"'{row.Name}': Fiyat 0'dan buyuk olmalidir.");
                    result.Skipped++; continue;
                }

                await _unitOfWork.Products.AddAsync(new Product
                {
                    Name = row.Name, Price = row.Price, StockCount = row.StockCount,
                    MinStockLevel = row.MinStockLevel > 0 ? row.MinStockLevel : 5, IsActive = true
                });
                result.Imported++;
            }

            if (result.Imported > 0) await _unitOfWork.CompleteAsync();
            return result;
        }
    }
}
