using ERP.API.Hubs;
using ERP.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ERP.API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<ErpHub> _hubContext;

        public NotificationService(IHubContext<ErpHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // Stok kritik seviyenin altına düştüğünde
        public async Task SendLowStockAlertAsync(int tenantId, string productName, int currentStock)
        {
            await SendToTenantAsync(
                tenantId,
                type: "LowStock",
                title: "⚠️ Kritik Stok Uyarısı",
                message: $"{productName} ürününde stok {currentStock} adede düştü!",
                severity: "warning",
                data: new { productName, currentStock });
        }

        // Yeni satış gerçekleştiğinde
        public async Task SendNewSaleNotificationAsync(int tenantId, decimal amount, string customerName)
        {
            await SendToTenantAsync(
                tenantId,
                type: "NewSale",
                title: "✅ Yeni Satış",
                message: $"{customerName} müşterisine {amount:C2} tutarında satış yapıldı.",
                severity: "success",
                data: new { amount, customerName });
        }

        // Yeni servis kaydı açıldığında
        public async Task SendNewServiceOrderAsync(int tenantId, string deviceName, string customerName)
        {
            await SendToTenantAsync(
                tenantId,
                type: "NewServiceOrder",
                title: "🔧 Yeni Servis Talebi",
                message: $"{customerName} - {deviceName} için servis talebi açıldı.",
                severity: "info",
                data: new { deviceName, customerName });
        }

        // Genel bildirim — diğer metodlar bunu çağırır
        public async Task SendToTenantAsync(int tenantId, string type, string title, string message,
            string severity = "info", object? data = null)
        {
            var notification = new ErpNotification
            {
                Type = type,
                Title = title,
                Message = message,
                Severity = severity,
                Data = data,
                Timestamp = DateTime.UtcNow
            };

            // Sadece ilgili firmanın kullanıcılarına gönder
            await _hubContext.Clients
                .Group($"Tenant_{tenantId}")
                .SendAsync("ReceiveNotification", notification);
        }
    }
}
