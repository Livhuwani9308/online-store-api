using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using online_store_api.Models.DTOs;
using online_store_api.Models.User;
using online_store_api.Services.Interfaces;

namespace online_store_api.Controllers
{
    public class AuthController(IAuthService service) : BaseController
    {
        private readonly IAuthService _service = service;

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _service.RegisterAsync(dto);

            return StatusCode(result.StatusCode, result);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _service.LoginAsync(dto);

            return StatusCode(result.StatusCode, result);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshRequestDto dto)
        {
            var result = await _service.RefreshTokenAsync(dto.RefreshToken);

            return StatusCode(result.StatusCode, result);
        }
    }
}
