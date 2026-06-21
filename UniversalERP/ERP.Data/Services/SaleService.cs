using AutoMapper;
using ERP.Core.DTOs.Sale;
using ERP.Core.Entities;
using ERP.Core.Interfaces;

namespace ERP.Data.Services
{
    public class SaleService : ISaleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLog;
        private readonly INotificationService _notification;

        public SaleService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IAuditLogService auditLog,
            INotificationService notification)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _auditLog = auditLog;
            _notification = notification;
        }

        public async Task<IEnumerable<SaleDto>> GetAllAsync()
        {
            var sales = await _unitOfWork.Sales.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<SaleDto>>(sales).ToList();
            foreach (var dto in dtos)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
                var customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId);
                dto.ProductName = product?.Name ?? "Silinmiş Ürün";
                dto.CustomerName = customer?.FullName ?? "Silinmiş Müşteri";
            }
            return dtos;
        }

        public async Task<SaleDto?> GetByIdAsync(int id)
        {
            var sale = await _unitOfWork.Sales.GetByIdAsync(id);
            if (sale == null) return null;
            var dto = _mapper.Map<SaleDto>(sale);
            var product = await _unitOfWork.Products.GetByIdAsync(sale.ProductId);
            var customer = await _unitOfWork.Customers.GetByIdAsync(sale.CustomerId);
            dto.ProductName = product?.Name ?? "Silinmiş Ürün";
            dto.CustomerName = customer?.FullName ?? "Silinmiş Müşteri";
            return dto;
        }

        public async Task<SaleDto> CreateSaleAsync(CreateSaleDto dto, int cashierId, string userId = "", string userEmail = "")
        {
            // 1. Ürünü kontrol et
            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null)
                throw new InvalidOperationException("Ürün bulunamadı.");
            if (product.StockCount < dto.Quantity)
                throw new InvalidOperationException(
                    $"Yetersiz stok. Mevcut: {product.StockCount}, İstenen: {dto.Quantity}");

            var oldStock = product.StockCount;

            // 2. Stoku düşür
            product.StockCount -= dto.Quantity;
            _unitOfWork.Products.Update(product);

            // 3. Satış kaydı
            var sale = new Sale
            {
                ProductId = dto.ProductId,
                CustomerId = dto.CustomerId,
                Quantity = dto.Quantity,
                TotalAmount = product.Price * dto.Quantity,
                PaymentMethod = dto.PaymentMethod,
                CashierId = cashierId,
                Status = "Completed"
            };
            await _unitOfWork.Sales.AddAsync(sale);

            // 4. Atomik kaydet (ACID)
            await _unitOfWork.CompleteAsync();

            var customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId);

            // 5. Audit Log
            await _auditLog.LogAsync(
                action: "Create",
                entityName: "Sale",
                entityId: sale.Id.ToString(),
                userId: userId,
                userEmail: userEmail,
                tenantId: sale.TenantId,
                newValues: new { sale.ProductId, sale.Quantity, sale.TotalAmount, sale.PaymentMethod });

            // 6. SignalR bildirimleri (fire & forget — hata olursa satışı bloklama)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _notification.SendNewSaleNotificationAsync(
                        sale.TenantId, sale.TotalAmount, customer?.FullName ?? "Müşteri");

                    // Stok kritik seviyeye düştüyse uyarı gönder
                    if (product.StockCount <= product.MinStockLevel)
                        await _notification.SendLowStockAlertAsync(
                            sale.TenantId, product.Name, product.StockCount);
                }
                catch { /* bildirim hatası satışı etkilemesin */ }
            });

            var resultDto = _mapper.Map<SaleDto>(sale);
            resultDto.ProductName = product.Name;
            resultDto.CustomerName = customer?.FullName ?? "";
            return resultDto;
        }

        public async Task<bool> CancelSaleAsync(int id, string userId = "", string userEmail = "")
        {
            var sale = await _unitOfWork.Sales.GetByIdAsync(id);
            if (sale == null || sale.Status == "Cancelled") return false;

            var product = await _unitOfWork.Products.GetByIdAsync(sale.ProductId);
            if (product != null)
            {
                product.StockCount += sale.Quantity;
                _unitOfWork.Products.Update(product);
            }

            sale.Status = "Cancelled";
            _unitOfWork.Sales.Update(sale);
            await _unitOfWork.CompleteAsync();

            await _auditLog.LogAsync("Cancel", "Sale", id.ToString(), userId, userEmail, sale.TenantId);
            return true;
        }

        // ISaleService uyumluluğu için eski imzaları da destekle
        Task<SaleDto> ISaleService.CreateSaleAsync(CreateSaleDto dto, int cashierId)
            => CreateSaleAsync(dto, cashierId);
        Task<bool> ISaleService.CancelSaleAsync(int id)
            => CancelSaleAsync(id);
    }
}
