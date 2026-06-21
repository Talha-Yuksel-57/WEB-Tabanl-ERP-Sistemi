namespace ERP.Core.Interfaces
{
    public interface INotificationService
    {
        Task SendLowStockAlertAsync(int tenantId, string productName, int currentStock);
        Task SendNewSaleNotificationAsync(int tenantId, decimal amount, string customerName);
        Task SendNewServiceOrderAsync(int tenantId, string deviceName, string customerName);
        Task SendToTenantAsync(int tenantId, string type, string title, string message, string severity = "info", object? data = null);
    }
}
