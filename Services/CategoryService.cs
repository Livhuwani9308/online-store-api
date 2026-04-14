using AutoMapper;
using Microsoft.EntityFrameworkCore;
using online_store_api.Common;
using online_store_api.Data;
using online_store_api.Helpers;
using online_store_api.Models.Category;
using online_store_api.Services.Interfaces;

namespace online_store_api.Services
{
    public class CategoryService(
        AppDbContext _db,
        IMapper mapper,
        IResponseHelper _response) : ICategoryService
    {
        public async Task<ServiceResponse<CategoryDto>> CreateAsync(CategoryDto model)
        {
            var category = mapper.Map<Category>(model);

            category.CreatedAt = DateTime.UtcNow;

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            var dto = mapper.Map<CategoryDto>(category);

            return _response.Create(true, 201, "Created", dto);
        }

        public async Task<ServiceResponse<IEnumerable<CategoryDto>>> GetAllAsync()
        {
            var categories = await _db.Categories
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            var dto = mapper.Map<IEnumerable<CategoryDto>>(categories);

            return _response.Create(true, 200, "Success", dto);
        }

        public async Task<ServiceResponse<CategoryDto>> GetByIdAsync(int id)
        {
            var category = await _db.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (category == null)
                return _response.Create<CategoryDto>(false, 404, "Not found", null);

            var dto = mapper.Map<CategoryDto>(category);

            return _response.Create(true, 200, "Success", dto);
        }

        public async Task<ServiceResponse<CategoryDto>> UpdateAsync(int id, CategoryDto model)
        {
            var category = await _db.Categories.FindAsync(id);

            if (category == null)
                return _response.Create<CategoryDto>(false, 404, "Not found", null);

            mapper.Map(model, category);

            await _db.SaveChangesAsync();

            var dto = mapper.Map<CategoryDto>(category);

            return _response.Create(true, 200, "Updated", dto);
        }

        public async Task<ServiceResponse<string>> DeleteAsync(int id)
        {
            var category = await _db.Categories.FindAsync(id);

            if (category == null)
                return _response.Create<string>(false, 404, "Not found", null);

            category.IsDeleted = true;

            await _db.SaveChangesAsync();

            return _response.Create<string>(true, 200, "Deleted", null);
        }
    }
}
