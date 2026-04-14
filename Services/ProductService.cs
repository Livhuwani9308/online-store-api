using AutoMapper;
using Microsoft.EntityFrameworkCore;
using online_store_api.Common;
using online_store_api.Data;
using online_store_api.Helpers;
using online_store_api.Models.Product;
using online_store_api.Services.Interfaces;

namespace online_store_api.Services
{
    public class ProductService(
        AppDbContext _db,
        IMapper mapper,
        IResponseHelper _response) : IProductService
    {
        public async Task<ServiceResponse<ProductDto>> CreateAsync(ProductDto model)
        {
            var product = mapper.Map<Product>(model);
            product.CreatedAt = DateTime.UtcNow;

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            if (model.Sizes.Any())
            {
                var sizes = model.Sizes.Select(s => new ProductSize
                {
                    ProductId = product.Id,
                    SizeValue = s.SizeValue,
                    StockQuantity = s.StockQuantity
                });

                _db.ProductSizes.AddRange(sizes);
                await _db.SaveChangesAsync();
            }

            return _response.Create(true, 201, "Created", mapper.Map<ProductDto>(product));
        }

        public async Task<ServiceResponse<IEnumerable<ProductDto>>> GetAllAsync()
        {
            var products = await _db.Products
                .Where(p => !p.IsDeleted)
                .ToListAsync();

            var result = new List<ProductDto>();

            foreach (var product in products)
            {
                var sizes = await _db.ProductSizes
                    .Where(s => s.ProductId == product.Id && !s.IsDeleted)
                    .ToListAsync();

                var dto = mapper.Map<ProductDto>(product);
                dto.Sizes = mapper.Map<List<ProductSizeDto>>(sizes);

                result.Add(dto);
            }

            return _response.Create<IEnumerable<ProductDto>>(true, 200, "Success", result);
        }

        public async Task<ServiceResponse<ProductDto>> GetByIdAsync(int id)
        {
            var product = await _db.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product == null)
                return _response.Create<ProductDto>(false, 404, "Not found", null);

            var sizes = await _db.ProductSizes
                .AsNoTracking()
                .Where(s => s.ProductId == id && !s.IsDeleted)
                .ToListAsync();

            var dto = mapper.Map<ProductDto>(product);
            dto.Sizes = mapper.Map<List<ProductSizeDto>>(sizes);

            return _response.Create(true, 200, "Success", dto);
        }

        public async Task<ServiceResponse<ProductDto>> UpdateAsync(int id, ProductDto model)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product == null)
                return _response.Create<ProductDto>(false, 404, "Not found", null);

            mapper.Map(model, product);

            await _db.SaveChangesAsync();

            return _response.Create(true, 200, "Updated", mapper.Map<ProductDto>(product));
        }

        public async Task<ServiceResponse<string>> DeleteAsync(int id)
        {
            var product = await _db.Products.FirstOrDefaultAsync(c => c.Id == id);

            if (product == null)
                return _response.Create<string>(false, 404, "Not found", null);

            product.IsDeleted = true;
            await _db.SaveChangesAsync();

            return _response.Create<string>(true, 200, "Deleted", null);
        }

        public async Task<ServiceResponse<IEnumerable<ProductDto>>> SearchAsync(ProductFilterDto filter)
        {
            var query = _db.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(filter.Search))
                query = query.Where(p => p.Name.Contains(filter.Search));

            if (!string.IsNullOrWhiteSpace(filter.Brand))
                query = query.Where(p => p.Brand == filter.Brand);

            if (!string.IsNullOrWhiteSpace(filter.Color))
                query = query.Where(p => p.Color == filter.Color);

            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId);

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var result = new List<ProductDto>();

            foreach (var product in products)
            {
                var sizes = await _db.ProductSizes
                    .AsNoTracking()
                    .Where(s => s.ProductId == product.Id && !s.IsDeleted)
                    .ToListAsync();

                var dto = mapper.Map<ProductDto>(product);
                dto.Sizes = mapper.Map<List<ProductSizeDto>>(sizes);

                result.Add(dto);
            }

            return _response.Create<IEnumerable<ProductDto>>(true, 200, "Search results", result);
        }
    }
}
