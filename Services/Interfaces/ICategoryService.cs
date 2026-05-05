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
        Task<ServiceResponse<CategoryDto>> CreateAsync(CategoryDto model);
        Task<ServiceResponse<List<Category>>> GetAllAsync();
        Task<ServiceResponse<CategoryDto>> UpdateAsync(int id, CategoryDto model);
        Task<ServiceResponse<string>> DeleteAsync(int id);
        Task<ServiceResponse<CategoryDto>> GetByIdAsync(int id);
    }
}
