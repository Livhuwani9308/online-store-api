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
        IResponseHelper _response,
        IMediaService _mediaService) : ICategoryService
    {
        public async Task<ServiceResponse<IEnumerable<CategoryDto>>> GetAllAsync(
            int? id,
            string? name,
            int page = 1,
            int pageSize = 10)
        {
            var query = _db.Categories
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .AsQueryable();

            if (id.HasValue)
            {
                query = query.Where(c => c.Id == id.Value);
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim().ToLower();

                query = query.Where(c =>
                    c.Name.ToLower().Contains(name));
            }

            var categories = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dto = mapper.Map<IEnumerable<CategoryDto>>(categories);

            return _response.Create(true, 200, "Categories retrieved successfully.", dto);
        }

        public async Task<ServiceResponse<CategoryDto>> CreateAsync(CategoryDto model, IFormFile? thumbnail)
        {
            var exists = await _db.Categories.AnyAsync(x => x.Name.ToLower() == model.Name.ToLower());

            if (exists)
                return _response.Create<CategoryDto>(false, 409, "Category already exists", null);

            var category = mapper.Map<Category>(model);
            category.CreatedAt = DateTime.UtcNow;

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            if (thumbnail != null)
            {
                var url = await _mediaService.UploadCategoryThumbnailAsync(category.Id, thumbnail);
                category.ThumbnailUrl = url;
                await _db.SaveChangesAsync();
            }

            var dto = mapper.Map<CategoryDto>(category);
            return _response.Create(true, 201, "Created", dto);
        }

        public async Task<ServiceResponse<CategoryDto>> UpdateAsync(CategoryDto model, IFormFile? thumbnail)
        {
            if (model.Id == null || model.Id <= 0)
                return _response.Create<CategoryDto>(false, 400, "Invalid category id.", null);

            var category = await _db.Categories.FirstOrDefaultAsync(x => x.Id == model.Id && !x.IsDeleted);

            if (category == null)
                return _response.Create<CategoryDto>(false, 404, "Category not found.", null);

            var normalizedName = model.Name.Trim().ToLower();

            var exists = await _db.Categories.AnyAsync(x =>
                x.Id != model.Id &&
                x.Name.ToLower() == normalizedName &&
                !x.IsDeleted);

            if (exists)
                return _response.Create<CategoryDto>(false, 409, "Category already exists", null);

            category.Name = model.Name;

            if (thumbnail != null)
            {
                var url = await _mediaService.UploadCategoryThumbnailAsync(category.Id, thumbnail);

                category.ThumbnailUrl = url;
            }

            await _db.SaveChangesAsync();

            var dto = mapper.Map<CategoryDto>(category);

            return _response.Create(true, 200, "Category updated successfully.", dto);
        }

        public async Task<ServiceResponse<string>> DeleteAsync(int id)
        {
            var category = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (category == null)
                return _response.Create<string>(false, 404, "Not found", null);

            category.IsDeleted = true;

            await _db.SaveChangesAsync();

            return _response.Create<string>(true, 200, "Deleted", null);
        }
    }
}
