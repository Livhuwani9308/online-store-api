using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using online_store_api.Models.User;
using online_store_api.Services.Interfaces;

namespace online_store_api.Controllers
{
    public class UserController(IUserService userService) : BaseController
    {
        private readonly IUserService _userService = userService;

        [HttpGet]
        public async Task<IActionResult> GetAll(
            int? id,
            string? email,
            string? firstName,
            string? lastName,
            int page = 1,
            int pageSize = 10)
        {
            var response = await _userService.GetAllAsync(
                id,
                email,
                firstName,
                lastName,
                page,
                pageSize);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser(
            [FromForm] CreateUserDto model,
            IFormFile? thumbnail)
        {
            var response = await _userService.CreateUserAsync(model, thumbnail);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPut]
        [Authorize(Roles = "Admin, Customer")]
        public async Task<IActionResult> UpdateUser([FromForm] UserDto model, IFormFile? thumbnail)
        {
            var response = await _userService.UpdateUserAsync(model, thumbnail);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("reset-password/{id}")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto model)
        {
            var response = await _userService.ResetPasswordAsync(id, model);

            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Customer")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var response = await _userService.DeleteUserAsync(id);

            return StatusCode(response.StatusCode, response);
        }
    }
}
