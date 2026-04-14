using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using online_store_api.Models.Category;
using online_store_api.Services.Interfaces;

namespace online_store_api.Controllers
{
    public class CategoryController(ICategoryService service) : BaseController
    {
        private readonly ICategoryService _service = service;

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var response = await _service.GetAllAsync();

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CategoryDto model)
        {
            var response = await _service.CreateAsync(model);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, CategoryDto model)
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
    }
}
