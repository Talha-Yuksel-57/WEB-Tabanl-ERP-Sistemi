using AutoMapper;
using ERP.Core.DTOs.ServiceOrder;
using ERP.Core.Entities;
using ERP.Core.Interfaces;

namespace ERP.Data.Services
{
    public class ServiceOrderService : IServiceOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ServiceOrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ServiceOrderDto>> GetAllAsync()
        {
            var orders = await _unitOfWork.ServiceOrders.GetAllAsync();
            return _mapper.Map<IEnumerable<ServiceOrderDto>>(orders);
        }

        public async Task<ServiceOrderDto?> GetByIdAsync(int id)
        {
            var order = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
            return order == null ? null : _mapper.Map<ServiceOrderDto>(order);
        }

        public async Task<ServiceOrderDto> CreateAsync(CreateServiceOrderDto dto)
        {
            var order = _mapper.Map<ServiceOrder>(dto);
            order.Status = "Beklemede";
            await _unitOfWork.ServiceOrders.AddAsync(order);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<ServiceOrderDto>(order);
        }

        public async Task<bool> UpdateStatusAsync(int id, string newStatus)
        {
            var order = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
            if (order == null) return false;

            order.Status = newStatus;
            _unitOfWork.ServiceOrders.Update(order);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var order = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
            if (order == null) return false;

            order.IsDeleted = true;
            _unitOfWork.ServiceOrders.Update(order);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
