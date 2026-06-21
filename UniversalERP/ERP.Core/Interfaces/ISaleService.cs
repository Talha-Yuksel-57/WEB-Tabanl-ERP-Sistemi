using ERP.Core.DTOs.Sale;

namespace ERP.Core.Interfaces
{
    public interface ISaleService
    {
        Task<IEnumerable<SaleDto>> GetAllAsync();
        Task<SaleDto?> GetByIdAsync(int id);

        // ACID transaction: Stok düş + Satış kayıt + Fatura oluştur
        Task<SaleDto> CreateSaleAsync(CreateSaleDto dto, int cashierId);
        Task<bool> CancelSaleAsync(int id);
    }
}
