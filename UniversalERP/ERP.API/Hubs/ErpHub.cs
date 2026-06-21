using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ERP.API.Hubs
{
    [Authorize]
    public class ErpHub : Hub
    {
        private readonly ILogger<ErpHub> _logger;

        public ErpHub(ILogger<ErpHub> logger)
        {
            _logger = logger;
        }

        // Kullanıcı bağlandığında kendi tenant grubuna katılır
        // Böylece bir firmanın bildirimleri diğerine gitmiyor
        public override async Task OnConnectedAsync()
        {
            var tenantId = Context.User?.FindFirst("TenantId")?.Value ?? "0";
            var userEmail = Context.User?.Identity?.Name ?? "Anonim";

            await Groups.AddToGroupAsync(Context.ConnectionId, $"Tenant_{tenantId}");

            _logger.LogInformation("SignalR bağlantısı: {Email} | TenantId: {TenantId}", userEmail, tenantId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var tenantId = Context.User?.FindFirst("TenantId")?.Value ?? "0";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Tenant_{tenantId}");
            await base.OnDisconnectedAsync(exception);
        }
    }

    // Bildirim modeli — frontend bu yapıyı alacak
    public class ErpNotification
    {
        public string Type { get; set; } = string.Empty;      // LowStock, NewSale, NewServiceOrder vb.
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "info";        // info, warning, success, error
        public object? Data { get; set; }                     // Ek veri (ürün ID, miktar vb.)
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
