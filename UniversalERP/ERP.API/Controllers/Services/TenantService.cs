namespace ERP.API.Services
{
    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetTenantId()
        {
            var tenantIdHeader = _httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-Id"].ToString();
            return int.TryParse(tenantIdHeader, out int tenantId) ? tenantId : 0;
        }
    }
}