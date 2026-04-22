using Microsoft.EntityFrameworkCore;
using online_store_api.Configuration;
using online_store_api.Models.User;

namespace online_store_api.Data.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAdminAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var seedAdmin = config.GetSection("SeedAdmin").Get<SeedAdminSettings>();

            if (seedAdmin == null)
                throw new Exception("SeedAdmin configuration is missing");

            await context.Database.MigrateAsync();

            var adminExists = await context.Users
                .AnyAsync(x => x.Email == seedAdmin.Email);

            if (adminExists) return;

            var adminRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Admin");

            if (adminRole == null)
                throw new Exception("Admin role not found");

            var admin = new User
            {
                FirstName = seedAdmin.FirstName,
                LastName = seedAdmin.LastName,
                Email = seedAdmin.Email,
                Phone = seedAdmin.Phone,
                Password = BCrypt.Net.BCrypt.HashPassword(seedAdmin.Password),
                RoleId = adminRole.Id,
                IsDeleted = false
            };

            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}
