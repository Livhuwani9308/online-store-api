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
        IMapper _mapper,
        IResponseHelper _response,
        IMediaService _mediaService)
        : IProductService
    {
        //public async Task<ServiceResponse<ProductDto>> CreateAsync(
        //    ProductDto model,
        //    IFormFileCollection? productImages)
        //{
        //    var categoryExists = await _db.Categories
        //        .AnyAsync(x => x.Id == model.CategoryId);

        //    if (!categoryExists)
        //    {
        //        return _response.Create<ProductDto>(
        //            false,
        //            404,
        //            "Category not found",
        //            null);
        //    }

        //    var product = _mapper.Map<Product>(model);

        //    product.CreatedAt = DateTime.UtcNow;

        //    _db.Products.Add(product);

        //    await _db.SaveChangesAsync();

        //    // Sizes
        //    if (model.Sizes.Any())
        //    {
        //        var sizes = model.Sizes.Select(x => new ProductSize
        //        {
        //            ProductId = product.Id,
        //            SizeValue = x.SizeValue,
        //            StockQuantity = x.StockQuantity
        //        });

        //        await _db.ProductSizes.AddRangeAsync(sizes);

        //        await _db.SaveChangesAsync();
        //    }

        //    // Images
        //    if (productImages != null && productImages.Count > 0)
        //    {
        //        var uploadedImages =
        //            await _mediaService.UploadProductMediaAsync(
        //                product.Id,
        //                productImages);

        //        var firstImage = uploadedImages.FirstOrDefault();

        //        if (firstImage != null)
        //        {
        //            product.ThumbnailUrl = firstImage.FileUrl;

        //            await _db.SaveChangesAsync();
        //        }
        //    }

        //    var dto = _mapper.Map<ProductDto>(product);

        //    dto.Sizes = model.Sizes;

        //    return _response.Create(
        //        true,
        //        201,
        //        "Product created successfully",
        //        dto);
        //}
        public async Task<ServiceResponse<ProductDto>> CreateAsync(
            ProductDto model,
            IFormFileCollection? productImages)
        {
            var categoryExists = await _db.Categories
                .AnyAsync(x => x.Id == model.CategoryId && !x.IsDeleted);

            if (!categoryExists)
            {
                return _response.Create<ProductDto>(
                    false,
                    404,
                    "Category not found",
                    null);
            }

            var product = _mapper.Map<Product>(model);

            product.CreatedAt = DateTime.UtcNow;

            _db.Products.Add(product);

            await _db.SaveChangesAsync();

            // ---------------- Sizes ----------------
            if (model.Sizes.Any())
            {
                var sizes = model.Sizes.Select(x => new ProductSize
                {
                    ProductId = product.Id,
                    SizeValue = x.SizeValue,
                    StockQuantity = x.StockQuantity
                });

                await _db.ProductSizes.AddRangeAsync(sizes);

                await _db.SaveChangesAsync();
            }

            // ---------------- Images ----------------
            List<ProductImage> uploadedImages = [];

            if (productImages != null && productImages.Count > 0)
            {
                uploadedImages = await _mediaService
                    .UploadProductMediaAsync(product.Id, productImages);

                var firstImage = uploadedImages.FirstOrDefault();

                if (firstImage != null)
                {
                    product.ThumbnailUrl = firstImage.FileUrl;

                    await _db.SaveChangesAsync();
                }
            }

            // ---------------- Response ----------------
            var dto = _mapper.Map<ProductDto>(product);

            dto.Sizes = model.Sizes;

            dto.Images = uploadedImages
                .Select(x => x.FileUrl)
                .ToList();

            dto.ThumbnailUrl = product.ThumbnailUrl;

            return _response.Create(
                true,
                201,
                "Product created successfully",
                dto);
        }

        public async Task<ServiceResponse<IEnumerable<ProductDto>>> GetAllAsync(
            string? search = null,
            string? brand = null,
            string? color = null,
            int? categoryId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? isAvailable = null,
            int page = 1,
            int pageSize = 10)
        {
            var query = _db.Products.AsNoTracking().Where(x => !x.IsDeleted);

            // Filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.Name.ToLower().Contains(search.ToLower()) ||
                    x.Description.ToLower().Contains(search.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(brand))
            {
                query = query.Where(x =>
                    x.Brand.ToLower() == brand.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(color))
            {
                query = query.Where(x =>
                    x.Color.ToLower() == color.ToLower());
            }

            if (categoryId.HasValue)
            {
                query = query.Where(x =>
                    x.CategoryId == categoryId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(x =>
                    x.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(x =>
                    x.Price <= maxPrice.Value);
            }

            if (isAvailable.HasValue)
            {
                query = query.Where(x =>
                    x.IsAvailable == isAvailable.Value);
            }

            var products = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            //var productIds = products.Select(x => x.Id).ToList();

            //var sizes = await _db.ProductSizes
            //    .AsNoTracking()
            //    .Where(x =>
            //        productIds.Contains(x.ProductId) &&
            //        !x.IsDeleted)
            //    .ToListAsync();

            //var result = products.Select(product =>
            //{
            //    var dto = _mapper.Map<ProductDto>(product);

            //    dto.Sizes = sizes
            //        .Where(x => x.ProductId == product.Id)
            //        .Select(x => new ProductSizeDto
            //        {
            //            SizeValue = x.SizeValue,
            //            StockQuantity = x.StockQuantity
            //        })
            //        .ToList();

            //    return dto;
            //}).ToList();
            var productIds = products.Select(x => x.Id).ToList();

            var sizes = await _db.ProductSizes
                .AsNoTracking()
                .Where(x =>
                    productIds.Contains(x.ProductId) &&
                    !x.IsDeleted)
                .ToListAsync();

            var images = await _db.ProductImages
                .AsNoTracking()
                .Where(x => productIds.Contains(x.ProductId))
                .ToListAsync();

            var result = products.Select(product =>
            {
                var dto = _mapper.Map<ProductDto>(product);

                dto.Sizes = sizes
                    .Where(x => x.ProductId == product.Id)
                    .Select(x => new ProductSizeDto
                    {
                        SizeValue = x.SizeValue,
                        StockQuantity = x.StockQuantity
                    })
                    .ToList();

                dto.Images = images
                    .Where(x => x.ProductId == product.Id)
                    .Select(x => x.FileUrl)
                    .ToList();

                return dto;

            }).ToList();

            return _response.Create<IEnumerable<ProductDto>>(true, 200, "Success", result);
        }

        //public async Task<ServiceResponse<ProductDto>> GetByIdAsync(int id)
        //{
        //    var product = await _db.Products
        //        .AsNoTracking()
        //        .FirstOrDefaultAsync(x =>
        //            x.Id == id &&
        //            !x.IsDeleted);

        //    if (product == null)
        //    {
        //        return _response.Create<ProductDto>(
        //            false,
        //            404,
        //            "Product not found",
        //            null);
        //    }

        //    var sizes = await _db.ProductSizes
        //        .AsNoTracking()
        //        .Where(x =>
        //            x.ProductId == id &&
        //            !x.IsDeleted)
        //        .ToListAsync();

        //    var dto = _mapper.Map<ProductDto>(product);

        //    dto.Sizes = sizes.Select(x => new ProductSizeDto
        //    {
        //        SizeValue = x.SizeValue,
        //        StockQuantity = x.StockQuantity
        //    }).ToList();

        //    return _response.Create(
        //        true,
        //        200,
        //        "Success",
        //        dto);
        //}
        public async Task<ServiceResponse<ProductDto>> GetByIdAsync(int id)
        {
            var product = await _db.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

            if (product == null)
            {
                return _response.Create<ProductDto>(
                    false,
                    404,
                    "Product not found",
                    null);
            }

            var sizes = await _db.ProductSizes
                .AsNoTracking()
                .Where(x =>
                    x.ProductId == id &&
                    !x.IsDeleted)
                .ToListAsync();

            var images = await _db.ProductImages
                .AsNoTracking()
                .Where(x => x.ProductId == id)
                .ToListAsync();

            var dto = _mapper.Map<ProductDto>(product);

            dto.Sizes = sizes.Select(x => new ProductSizeDto
            {
                SizeValue = x.SizeValue,
                StockQuantity = x.StockQuantity
            }).ToList();

            dto.Images = images
                .Select(x => x.FileUrl)
                .ToList();

            return _response.Create(
                true,
                200,
                "Success",
                dto);
        }

        public async Task<ServiceResponse<ProductDto>> UpdateAsync(
            ProductDto model,
            IFormFileCollection? productImages)
        {
            var product = await _db.Products
                .FirstOrDefaultAsync(x =>
                    x.Id == model.Id &&
                    !x.IsDeleted);

            if (product == null)
            {
                return _response.Create<ProductDto>(
                    false,
                    404,
                    "Product not found",
                    null);
            }

            product.Name = model.Name;
            product.Brand = model.Brand;
            product.Description = model.Description;
            product.Price = model.Price;
            product.CategoryId = model.CategoryId;
            product.Color = model.Color;

            // Remove old sizes
            var existingSizes = await _db.ProductSizes
                .Where(x => x.ProductId == product.Id)
                .ToListAsync();

            _db.ProductSizes.RemoveRange(existingSizes);

            // Add new sizes
            if (model.Sizes.Any())
            {
                var newSizes = model.Sizes.Select(x => new ProductSize
                {
                    ProductId = product.Id,
                    SizeValue = x.SizeValue,
                    StockQuantity = x.StockQuantity
                });

                await _db.ProductSizes.AddRangeAsync(newSizes);
            }

            //// Upload new images
            //if (productImages != null && productImages.Count > 0)
            //{
            //    var uploadedImages =
            //        await _mediaService.UploadProductMediaAsync(
            //            product.Id,
            //            productImages);

            //    var firstImage = uploadedImages.FirstOrDefault();

            //    if (firstImage != null)
            //    {
            //        product.ThumbnailUrl = firstImage.FileUrl;
            //    }
            //}

            //await _db.SaveChangesAsync();

            //var dto = _mapper.Map<ProductDto>(product);

            //dto.Sizes = model.Sizes;

            //return _response.Create(
            //    true,
            //    200,
            //    "Product updated successfully",
            //    dto);
            List<ProductImage> uploadedImages = [];

            if (productImages != null && productImages.Count > 0)
            {
                uploadedImages = await _mediaService
                    .UploadProductMediaAsync(
                        product.Id,
                        productImages);

                var firstImage = uploadedImages.FirstOrDefault();

                if (firstImage != null)
                {
                    product.ThumbnailUrl = firstImage.FileUrl;
                }
            }

            await _db.SaveChangesAsync();

            var allImages = await _db.ProductImages
                .Where(x => x.ProductId == product.Id)
                .ToListAsync();

            var dto = _mapper.Map<ProductDto>(product);

            dto.Sizes = model.Sizes;

            dto.Images = allImages
                .Select(x => x.FileUrl)
                .ToList();

            return _response.Create(
                true,
                200,
                "Product updated successfully",
                dto);
        }

        public async Task<ServiceResponse<string>> DeleteAsync(int id)
        {
            var product = await _db.Products
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

            if (product == null)
            {
                return _response.Create<string>(
                    false,
                    404,
                    "Product not found",
                    null);
            }

            product.IsDeleted = true;

            var sizes = await _db.ProductSizes
                .Where(x => x.ProductId == id)
                .ToListAsync();

            foreach (var size in sizes)
            {
                size.IsDeleted = true;
            }

            await _db.SaveChangesAsync();

            return _response.Create<string>(
                true,
                200,
                "Product deleted successfully",
                null);
        }
    }
}
