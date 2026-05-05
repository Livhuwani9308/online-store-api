using online_store_api.Common;
using online_store_api.Models.Category;

namespace online_store_api.Services.Interfaces
{
    public interface ICategoryService
    {
        //Task<ServiceResponse<CategoryDto>> CreateAsync(CreateCategoryDto model);
        //Task<ServiceResponse<IEnumerable<CategoryDto>>> GetAllAsync();
        //Task<ServiceResponse<CategoryDto>> UpdateAsync(int id, CreateCategoryDto model);
        //Task<ServiceResponse<string>> DeleteAsync(int id);
        Task<ServiceResponse<CategoryDto>> CreateAsync(CategoryDto model, IFormFile? thumbnail);
        Task<ServiceResponse<IEnumerable<CategoryDto>>> GetAllAsync();
        Task<ServiceResponse<CategoryDto>> UpdateAsync(CategoryDto model, IFormFile? thumbnail);
        Task<ServiceResponse<string>> DeleteAsync(int id);
        Task<ServiceResponse<CategoryDto>> GetByIdAsync(int id);
    }
}
