namespace ERP.Core.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(
            string action,
            string entityName,
            string entityId,
            string userId,
            string userEmail,
            int tenantId,
            object? oldValues = null,
            object? newValues = null,
            string? ipAddress = null);
    }
}
