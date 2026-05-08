using online_store_api.Common;
using online_store_api.Models.Product;

namespace online_store_api.Services.Interfaces
{
    public interface IProductService
    {
        Task<ServiceResponse<ProductDto>> CreateAsync(ProductDto model, IFormFileCollection? productImages);

        Task<ServiceResponse<IEnumerable<ProductDto>>> GetAllAsync(
            string? search = null,
            string? brand = null,
            string? color = null,
            int? categoryId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? isAvailable = null,
            int page = 1,
            int pageSize = 10);

        Task<ServiceResponse<ProductDto>> GetByIdAsync(int id);

        Task<ServiceResponse<ProductDto>> UpdateAsync(ProductDto model, IFormFileCollection? productImages);

        Task<ServiceResponse<string>> DeleteAsync(int id);
    }
}
