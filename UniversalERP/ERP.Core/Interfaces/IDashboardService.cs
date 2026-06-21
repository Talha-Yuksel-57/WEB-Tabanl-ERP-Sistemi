using ERP.Core.DTOs.Dashboard;

namespace ERP.Core.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardAsync();
    }
}
