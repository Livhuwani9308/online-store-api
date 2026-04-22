using Microsoft.AspNetCore.Mvc;
using online_store_api.Models.User;
using online_store_api.Services.Interfaces;

namespace online_store_api.Controllers
{
    public class UserController(IUserService userService) : BaseController
    {
        private readonly IUserService _userService = userService;

        //[AllowAnonymous]
        //[HttpPost("user-register")]
        //public async Task<IActionResult> Register(User model)
        //{
        //    var response = await _userService.RegisterAsync(model);

        //    return StatusCode(response.StatusCode, response);
        //}

        //[AllowAnonymous]
        //[HttpPost("user-login")]
        //public async Task<IActionResult> Login(LoginDto model)
        //{
        //    var response = await _userService.LoginAsync(model);

        //    return StatusCode(response.StatusCode, response);
        //}

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
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDto model)
        {
            var response = await _userService.UpdateUserAsync(id, model);

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
