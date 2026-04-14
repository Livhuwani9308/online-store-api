using online_store_api.Common;
using online_store_api.Models.Product;

namespace online_store_api.Services.Interfaces
{
    public interface IProductService
    {
        Task<ServiceResponse<ProductDto>> CreateAsync(ProductDto model);
        Task<ServiceResponse<IEnumerable<ProductDto>>> GetAllAsync();
        Task<ServiceResponse<ProductDto>> GetByIdAsync(int id);
        Task<ServiceResponse<ProductDto>> UpdateAsync(int id, ProductDto model);
        Task<ServiceResponse<string>> DeleteAsync(int id);
        Task<ServiceResponse<IEnumerable<ProductDto>>> SearchAsync(ProductFilterDto filter);
    }
}
