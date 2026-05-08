using online_store_api.Common;
using online_store_api.Models.Category;

namespace online_store_api.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<ServiceResponse<CategoryDto>> CreateAsync(CategoryDto model, IFormFile? thumbnail);

        Task<ServiceResponse<IEnumerable<CategoryDto>>> GetAllAsync(
            int? id,
            string? name,
            int page = 1,
            int pageSize = 10);

        Task<ServiceResponse<CategoryDto>> UpdateAsync(CategoryDto model, IFormFile? thumbnail);

        Task<ServiceResponse<string>> DeleteAsync(int id);
    }
}
