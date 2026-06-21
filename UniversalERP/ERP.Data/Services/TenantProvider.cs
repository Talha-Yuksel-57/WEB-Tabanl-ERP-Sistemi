using ERP.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ERP.Data.Services
{
    public class TenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetTenantId()
        {
            // API isteği yapılırken Header kısmında 'X-Tenant-Id' aranacak
            var headerValue = _httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-Id"].ToString();

            return int.TryParse(headerValue, out var tenantId) ? tenantId : 0;
        }
    }
}
