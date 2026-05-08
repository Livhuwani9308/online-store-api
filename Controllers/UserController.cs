using Microsoft.AspNetCore.Mvc;
using online_store_api.Models.User;
using online_store_api.Services.Interfaces;

namespace online_store_api.Controllers
{
    public class UserController(IUserService userService) : BaseController
    {
        private readonly IUserService _userService = userService;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _userService.GetUsersListAsync();

            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUser(SearchUserModelDto model)
        {
            var response = await _userService.SearchUserAsync(model);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{id}")]
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

        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var response = await _userService.DeleteUserAsync(id);

            return StatusCode(response.StatusCode, response);
        }
    }
}
