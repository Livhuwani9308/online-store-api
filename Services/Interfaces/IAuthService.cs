using online_store_api.Common;
using online_store_api.Models.DTOs;
using online_store_api.Models.User;

namespace online_store_api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResponse<bool>> RegisterAsync(RegisterDto dto);
        Task<ServiceResponse<AuthResponseDto>> LoginAsync(LoginDto dto);
        Task<ServiceResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken);
    }
}