using Microsoft.EntityFrameworkCore;
using online_store_api.Common;
using online_store_api.Data;
using online_store_api.Helpers;
using online_store_api.Models;
using online_store_api.Models.DTOs;
using online_store_api.Models.User;
using online_store_api.Services.Interfaces;

namespace online_store_api.Services
{
    public class AuthService(
        AppDbContext context,
        JwtTokenHelper jwt,
        IResponseHelper response) : IAuthService
    {
        private readonly AppDbContext _context = context;
        private readonly JwtTokenHelper _jwt = jwt;
        private readonly IResponseHelper _response = response;

        public async Task<ServiceResponse<bool>> RegisterAsync(RegisterDto dto)
        {
            var exists = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (exists != null)
                return _response.CreateResponse(
                    false,
                    400,
                    "Email already exists",
                    false);

            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Customer");

            if (role == null)
                return _response.CreateResponse(
                    false,
                    400,
                    "Role not found",
                    false);

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = role.Id
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return _response.CreateResponse(
                true,
                201,
                "Registration successful",
                true);
        }

        public async Task<ServiceResponse<AuthResponseDto>> LoginAsync(LoginDto dto)
        {
            var userData = await (
                from u in _context.Users
                join r in _context.Roles on u.RoleId equals r.Id
                where u.Email == dto.Email
                select new
                {
                    u.Id,
                    u.Email,
                    u.Password,
                    u.IsDeleted,
                    RoleName = r.Name
                }
            ).FirstOrDefaultAsync();

            if (userData == null || userData.IsDeleted)
                return _response.CreateResponse<AuthResponseDto>(
                    false,
                    401,
                    "Invalid credentials",
                    null);
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, userData.Password))
                return _response.CreateResponse<AuthResponseDto>(
                    false,
                    401,
                    "Invalid credentials",
                    null);

            var (accessToken, expires) = _jwt.GenerateToken(
                userData.Id,
                userData.Email,
                userData.RoleName);

            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                Expires = DateTime.UtcNow.AddDays(3),
                UserId = userData.Id
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            var responseDto = new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                Email = userData.Email,
                Role = userData.RoleName,
                ExpiresAt = expires
            };

            return _response.CreateResponse(
                true,
                200,
                "Login successful",
                responseDto);
        }

        public async Task<ServiceResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(t =>
                    t.Token == refreshToken &&
                    !t.IsRevoked);

            if (token == null || token.Expires < DateTime.UtcNow)
                return _response.CreateResponse<AuthResponseDto>(
                    false,
                    401,
                    "Invalid refresh token",
                    null);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == token.UserId);
            if (user == null || user.IsDeleted)
                return _response.CreateResponse<AuthResponseDto>(
                    false,
                    401,
                    "Invalid user",
                    null);

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == user.RoleId);
            if (role == null)
                return _response.CreateResponse<AuthResponseDto>(
                    false,
                    401,
                    "Invalid role",
                    null);

            var (accessToken, expires) = _jwt.GenerateToken(
                user.Id,
                user.Email,
                role!.Name);

            var responseDto = new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = token.Token,
                Email = user.Email,
                Role = role.Name,
                ExpiresAt = expires
            };

            return _response.CreateResponse(
                true,
                200,
                "Token refreshed",
                responseDto);
        }
    }
}