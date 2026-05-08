using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using online_store_api.Models.Product;
using online_store_api.Services.Interfaces;

namespace online_store_api.Controllers
{
    public class ProductController(IProductService service) : BaseController
    {
        private readonly IProductService _service = service;

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(
            string? search,
            string? brand,
            string? color,
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            bool? isAvailable,
            int page = 1,
            int pageSize = 10)
        {
            var response = await _service.GetAllAsync(
                search,
                brand,
                color,
                categoryId,
                minPrice,
                maxPrice,
                isAvailable,
                page,
                pageSize);

            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetByIdAsync(id);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] ProductDto model, [FromForm] IFormFileCollection? productImages)
        {
            var response = await _service.CreateAsync(model, productImages);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromForm] ProductDto model, [FromForm] IFormFileCollection? productImages)
        {
            var response = await _service.UpdateAsync(model, productImages);

            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeleteAsync(id);

            return StatusCode(response.StatusCode, response);
        }
    }
}
