using AutoMapper;
using Microsoft.EntityFrameworkCore;
using online_store_api.Common;
using online_store_api.Data;
using online_store_api.Helpers;
using online_store_api.Models.User;
using online_store_api.Services.Interfaces;

namespace online_store_api.Services
{
    public class UserService(AppDbContext _db, IResponseHelper _response, IMapper mapper, IMediaService _mediaService) : IUserService
    {
        public async Task<ServiceResponse<string>> ResetPasswordAsync(int id, ResetPasswordDto model)
        {
            try
            {
                if (
                    model.Id != id ||
                    model == null ||
                    string.IsNullOrWhiteSpace(model.CurrentPassword) ||
                    string.IsNullOrWhiteSpace(model.NewPassword) ||
                    string.IsNullOrWhiteSpace(model.ConfirmPassword)
                    )
                {
                    return _response.Create<string>(false, 400, "Invalid request.", null);
                }

                if (model.NewPassword != model.ConfirmPassword)
                    return _response.Create<string>(false, 422, "Incorrect password.", null);

                var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == model.Id);

                if (user == null)
                    return _response.Create<string>(false, 404, "User does not exist.", null);

                if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password))
                    return _response.Create<string>(false, 400, "Incorrect password.", null);

                user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

                _db.Users.Update(user);
                await _db.SaveChangesAsync();

                return _response.Create<string>(true, 200, "Password reset successful.", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return _response.Create<string>(false, 500, "An unexpected error occurred.", null);
            }
        }

        public async Task<ServiceResponse<IEnumerable<UserDto>>> GetAllAsync(
            int? id,
            string? email,
            string? firstName,
            string? lastName,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                var query = _db.Users
                    .AsNoTracking()
                    .Where(u => !u.IsDeleted)
                    .AsQueryable();

                if (id.HasValue)
                {
                    query = query.Where(u => u.Id == id.Value);
                }

                if (!string.IsNullOrWhiteSpace(email))
                {
                    email = email.Trim().ToLower();

                    query = query.Where(u =>
                        u.Email.ToLower().Contains(email));
                }

                if (!string.IsNullOrWhiteSpace(firstName))
                {
                    firstName = firstName.Trim().ToLower();

                    query = query.Where(u =>
                        u.FirstName.ToLower().Contains(firstName));
                }

                if (!string.IsNullOrWhiteSpace(lastName))
                {
                    lastName = lastName.Trim().ToLower();

                    query = query.Where(u =>
                        u.LastName.ToLower().Contains(lastName));
                }

                var users = await query
                    .OrderByDescending(u => u.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(user => new UserDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Phone = user.Phone,
                        ThumbnailUrl = user.ThumbnailUrl
                    })
                    .ToListAsync();

                return _response.Create<IEnumerable<UserDto>>(
                    true,
                    200,
                    "Users retrieved successfully.",
                    users);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return _response.Create<IEnumerable<UserDto>>(
                    false,
                    500,
                    "An unexpected error occurred.",
                    null);
            }
        }

        public async Task<ServiceResponse<UserDto>> UpdateUserAsync(
            UserDto model,
            IFormFile? thumbnail)
        {
            try
            {
                var user = await _db.Users
                    .FirstOrDefaultAsync(u =>
                        u.Id == model.Id &&
                        !u.IsDeleted);

                if (user == null)
                {
                    return _response.Create<UserDto>(
                        false,
                        404,
                        "User not found.",
                        null);
                }

                user.FirstName =
                    string.IsNullOrWhiteSpace(model.FirstName)
                        ? user.FirstName
                        : model.FirstName;

                user.LastName =
                    string.IsNullOrWhiteSpace(model.LastName)
                        ? user.LastName
                        : model.LastName;

                user.Phone =
                    string.IsNullOrWhiteSpace(model.Phone)
                        ? user.Phone
                        : model.Phone;

                user.Email =
                    string.IsNullOrWhiteSpace(model.Email)
                        ? user.Email
                        : model.Email;

                if (thumbnail != null)
                {
                    var url = await _mediaService.UploadUserThumbnailAsync(user.Id, thumbnail);

                    user.ThumbnailUrl = url;
                }

                await _db.SaveChangesAsync();

                var dto = mapper.Map<UserDto>(user);

                return _response.Create(
                    true,
                    200,
                    "User updated successfully.",
                    dto);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return _response.Create<UserDto>(
                    false,
                    500,
                    "An unexpected error occurred.",
                    null);
            }
        }

        public async Task<ServiceResponse<string>> DeleteUserAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return _response.Create<string>(false, 400, "Invalid input data.", null);

                var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.IsDeleted == false);

                if (user == null)
                    return _response.Create<string>(false, 400, "User not found.", null);

                user.IsDeleted = true;

                _db.Users.Update(user);
                await _db.SaveChangesAsync();

                return _response.Create<string>(true, 200, "User deleted successfully.", null);
            }
            catch (Exception ex)
            {
                return _response.Create<string>(false, 500, ex.Message, null);
            }
        }
    }
}