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
        public async Task<IActionResult> GetAll()
        {
            var response = await _service.GetAllAsync();

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
        public async Task<IActionResult> Create(ProductDto model)
        {
            var response = await _service.CreateAsync(model);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, ProductDto model)
        {
            var response = await _service.UpdateAsync(id, model);

            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeleteAsync(id);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("filter")]
        [AllowAnonymous]
        public async Task<IActionResult> Filter(ProductFilterDto filter)
        {
            var response = await _service.SearchAsync(filter);

            return StatusCode(response.StatusCode, response);
        }
    }
}
