using Microsoft.EntityFrameworkCore;
using online_store_api.Common;
using online_store_api.Data;
using online_store_api.Helpers;
using online_store_api.Models.User;
using online_store_api.Services.Interfaces;

namespace online_store_api.Services
{
    public class UserService(AppDbContext _db, IResponseHelper _response) : IUserService
    {
        //public async Task<ServiceResponse<UserDto>> LoginAsync(LoginDto model)
        //{
        //    try
        //    {
        //        if (model == null)
        //            return _response.Create<UserDto>(false, 400, "Invalid request", null);

        //        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email.ToLower());

        //        if (user == null)
        //            return _response.Create<UserDto>(false, 401, "Invalid email or password.", null);

        //        if (!BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
        //            return _response.Create<UserDto>(false, 401, "Invalid email or password.", null);

        //        var response = new UserDto()
        //        {
        //            Id = user.Id,
        //            FirstName = user.FirstName,
        //            LastName = user.LastName,
        //            Email = user.Email,
        //            Phone = user.Phone,
        //        };

        //        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == user.RoleId);

        //        var token = _token.GenerateToken(response.Id, response.Email, role?.Name ?? "User");

        //        return _response.Create(true, 200, "Login successful.", response);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        return _response.Create<UserDto>(false, 500, "An unexpected error occurred.", null);
        //    }
        //}

        //public async Task<ServiceResponse<UserDto>> RegisterAsync(User model)
        //{
        //    try
        //    {
        //        if (
        //            model == null ||
        //            string.IsNullOrWhiteSpace(model.FirstName) ||
        //            string.IsNullOrWhiteSpace(model.LastName) ||
        //            string.IsNullOrWhiteSpace(model.Email) ||
        //            string.IsNullOrWhiteSpace(model.Phone) ||
        //            string.IsNullOrWhiteSpace(model.Password)
        //            )
        //        {
        //            return _response.Create<UserDto>(false, 400, "Invalid request.", null);
        //        }

        //        var userExists = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email.ToLower());

        //        if (userExists != null)
        //            return _response.Create<UserDto>(false, 409, "Duplicate record found.", null); // 400 - test 409

        //        model.Email = model.Email.ToLower();
        //        model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

        //        _db.Users.Add(model);
        //        await _db.SaveChangesAsync();

        //        var responseData = new UserDto()
        //        {
        //            Id = model.Id,
        //            FirstName = model.FirstName,
        //            LastName = model.LastName,
        //            Phone = model.Phone,
        //            Email = model.Email
        //        };

        //        return _response.Create(true, 201, "User created successfully.", responseData);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        return _response.Create<UserDto>(false, 500, "An unexpected error occurred.", null);
        //    }
        //}

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

        public async Task<ServiceResponse<IEnumerable<UserDto>>> GetUsersListAsync()
        {
            try
            {
                var users = await _db.Users.Select(user => new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone
                }).ToListAsync();

                return _response.Create<IEnumerable<UserDto>>(true, 200, "Users retrieved successfully.", users);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return _response.Create<IEnumerable<UserDto>>(false, 500, "An unexpected error occurred.", null);
            }
        }

        public async Task<ServiceResponse<UserDto>> SearchUserAsync(SearchUserModelDto model)
        {
            try
            {
                var query = await _db.Users.AsQueryable().Where(u => (u.Id == model.Id || u.Email == model.Email.ToLower()) && u.IsDeleted == false).FirstOrDefaultAsync();

                if (query != null)
                {
                    var response = new UserDto()
                    {
                        Id = query.Id,
                        FirstName = query.FirstName,
                        LastName = query.LastName,
                        Email = query.Email,
                        Phone = query.Phone
                    };
                    return _response.Create(true, 200, "Users retrieved successfully.", response);
                }
                return _response.Create<UserDto>(false, 404, "User not found.", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return _response.Create<UserDto>(false, 500, "An unexpected error occurred.", null);
            }
        }

        public async Task<ServiceResponse<UserDto>> UpdateUserAsync(int id, UserDto model)
        {
            try
            {
                if (id != model.Id)
                    return _response.Create<UserDto>(false, 400, "Invalid request.", null);

                var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.IsDeleted == false);

                if (user == null)
                    return _response.Create<UserDto>(false, 404, "User not found.", null);

                user.FirstName = string.IsNullOrWhiteSpace(model.FirstName) ? user.FirstName : model.FirstName;
                user.LastName = string.IsNullOrWhiteSpace(model.LastName) ? user.LastName : model.LastName;
                user.Phone = string.IsNullOrWhiteSpace(model.Phone) ? user.Phone : model.Phone;
                user.Email = string.IsNullOrWhiteSpace(model.Email) ? user.Email : model.Email;

                _db.Users.Update(user);
                await _db.SaveChangesAsync();

                var updatedUserDto = new UserDto()
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                };

                return _response.Create(true, 200, "User updated successfully.", updatedUserDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return _response.Create<UserDto>(false, 500, "An unexpected error occurred.", null);
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