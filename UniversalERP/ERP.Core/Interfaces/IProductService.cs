using ERP.Core.DTOs.Product;

namespace ERP.Core.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<ProductDto?> GetByIdAsync(int id);
        Task<ProductDto> CreateAsync(CreateProductDto dto);
        Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto);
        Task<bool> UpdateStockAsync(int id, int newStock);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<ProductDto>> GetLowStockAsync();
    }
}
