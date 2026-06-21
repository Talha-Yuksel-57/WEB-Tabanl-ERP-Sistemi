using ERP.Core.Entities;
using ERP.Core.Interfaces;
using System.Text.Json;

namespace ERP.Data.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly AppDbContext _context;

        public AuditLogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(
            string action,
            string entityName,
            string entityId,
            string userId,
            string userEmail,
            int tenantId,
            object? oldValues = null,
            object? newValues = null,
            string? ipAddress = null)
        {
            var log = new AuditLog
            {
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                UserId = userId,
                UserEmail = userEmail,
                TenantId = tenantId,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
