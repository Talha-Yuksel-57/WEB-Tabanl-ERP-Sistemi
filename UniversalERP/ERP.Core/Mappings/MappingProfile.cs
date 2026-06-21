using AutoMapper;
using ERP.Core.DTOs.Customer;
using ERP.Core.DTOs.Product;
using ERP.Core.DTOs.ProjectTask;
using ERP.Core.DTOs.Sale;
using ERP.Core.DTOs.ServiceOrder;
using ERP.Core.Entities;

namespace ERP.Core.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // --- PRODUCT ---
            CreateMap<Product, ProductDto>();
            CreateMap<CreateProductDto, Product>();
            CreateMap<UpdateProductDto, Product>();

            // --- CUSTOMER ---
            CreateMap<Customer, CustomerDto>();
            CreateMap<CreateCustomerDto, Customer>();
            CreateMap<UpdateCustomerDto, Customer>();

            // --- SALE ---
            CreateMap<Sale, SaleDto>()
                .ForMember(dest => dest.ProductName, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerName, opt => opt.Ignore());
            CreateMap<CreateSaleDto, Sale>();

            // --- SERVICE ORDER ---
            CreateMap<ServiceOrder, ServiceOrderDto>();
            CreateMap<CreateServiceOrderDto, ServiceOrder>();

            // --- PROJECT TASK ---
            CreateMap<Entities.ProjectTask, ProjectTaskDto>();
            CreateMap<CreateProjectTaskDto, Entities.ProjectTask>();
            CreateMap<UpdateProjectTaskDto, Entities.ProjectTask>();
        }
    }
}
