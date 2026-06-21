using AutoMapper;
using ERP.Core.DTOs.Customer;
using ERP.Core.Entities;
using ERP.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ERP.Data.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomerService(
            IUnitOfWork unitOfWork, 
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllAsync()
        {
            var customers = await _unitOfWork.Customers.GetAllAsync();
            return _mapper.Map<IEnumerable<CustomerDto>>(customers);
        }

        public async Task<CustomerDto?> GetByIdAsync(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            return customer == null ? null : _mapper.Map<CustomerDto>(customer);
        }

        public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
        {
            // ✅ TenantId'yi token'dan al
            var tenantId = GetTenantIdFromContext();

            var customer = _mapper.Map<Customer>(dto);
            customer.TenantId = tenantId;
            
            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<CustomerDto>(customer);
        }

        public async Task<CustomerDto?> UpdateAsync(int id, UpdateCustomerDto dto)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null) return null;

            _mapper.Map(dto, customer);
            _unitOfWork.Customers.Update(customer);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<CustomerDto>(customer);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null) return false;

            customer.IsDeleted = true;
            _unitOfWork.Customers.Update(customer);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        // ✅ Token'dan TenantId çıkar
        private int GetTenantIdFromContext()
        {
            var tenantIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst("TenantId")?.Value;
            
            return int.TryParse(tenantIdClaim, out var tenantId) ? tenantId : 0;
        }
    }
}
