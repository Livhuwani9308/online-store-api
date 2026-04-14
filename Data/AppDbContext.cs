using Microsoft.EntityFrameworkCore;
using online_store_api.Models;
using online_store_api.Models.Category;
using online_store_api.Models.Product;
using online_store_api.Models.User;

namespace online_store_api.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<ProductSize> ProductSizes => Set<ProductSize>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //builder.Entity<User>()
            //    .Property(u => u.RowVersion)
            //    .IsRowVersion();

            builder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            builder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Customer" }
            );

            builder.Entity<Category>()
                .HasKey(c => c.Id);

            builder.Entity<Product>()
                .HasKey(p => p.Id);

            builder.Entity<ProductSize>()
                .HasKey(ps => ps.Id);

            builder.Entity<ProductSize>()
                .HasIndex(ps => new { ps.ProductId, ps.SizeValue })
                .IsUnique();

            builder.Entity<Product>()
                .HasIndex(p => p.CategoryId);

            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            builder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();
        }
    }
}
