using AutoMapper;
using ERP.Core.DTOs.Product;
using ERP.Core.Entities;
using ERP.Core.Interfaces;

namespace ERP.Data.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            return product == null ? null : _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            var product = _mapper.Map<Product>(dto);
            product.IsActive = true;
            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null) return null;

            _mapper.Map(dto, product);
            _unitOfWork.Products.Update(product);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<bool> UpdateStockAsync(int id, int newStock)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null) return false;

            product.StockCount = newStock;
            _unitOfWork.Products.Update(product);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null) return false;

            // Soft delete — gerçekten silmiyoruz
            product.IsDeleted = true;
            _unitOfWork.Products.Update(product);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<IEnumerable<ProductDto>> GetLowStockAsync()
        {
            var products = await _unitOfWork.Products.FindAsync(
                p => p.StockCount <= p.MinStockLevel && !p.IsDeleted);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }
    }
}
