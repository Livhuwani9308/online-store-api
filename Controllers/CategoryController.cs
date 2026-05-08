using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using online_store_api.Models.Category;
using online_store_api.Services.Interfaces;

namespace online_store_api.Controllers
{
    public class CategoryController(ICategoryService _service) : BaseController
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var response = await _service.GetAllAsync();

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] string name, IFormFile thumbnail)
        {
            var model = new CategoryDto { Name = name };

            var response = await _service.CreateAsync(model, thumbnail);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromForm] CategoryDto model, IFormFile? thumbnail)
        {
            var response = await _service.UpdateAsync(model, thumbnail);

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
