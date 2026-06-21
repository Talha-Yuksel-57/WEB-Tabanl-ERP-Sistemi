using ERP.Core.DTOs.ServiceOrder;

namespace ERP.Core.Interfaces
{
    public interface IServiceOrderService
    {
        Task<IEnumerable<ServiceOrderDto>> GetAllAsync();
        Task<ServiceOrderDto?> GetByIdAsync(int id);
        Task<ServiceOrderDto> CreateAsync(CreateServiceOrderDto dto);
        Task<bool> UpdateStatusAsync(int id, string newStatus);
        Task<bool> DeleteAsync(int id);
    }
}
