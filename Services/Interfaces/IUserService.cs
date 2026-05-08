using online_store_api.Common;
using online_store_api.Models.User;

namespace online_store_api.Services.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResponse<IEnumerable<UserDto>>> GetUsersListAsync();
        Task<ServiceResponse<UserDto>> SearchUserAsync(SearchUserModelDto model);
        Task<ServiceResponse<UserDto>> UpdateUserAsync(UserDto model, IFormFile? thumbnail);
        Task<ServiceResponse<string>> ResetPasswordAsync(int id, ResetPasswordDto model);
        Task<ServiceResponse<string>> DeleteUserAsync(int id);
    }
}