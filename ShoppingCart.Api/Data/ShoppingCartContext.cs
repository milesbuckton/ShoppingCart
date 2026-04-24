using Microsoft.EntityFrameworkCore;
using ShoppingCart.Api.Models;

namespace ShoppingCart.Api.Data
{
    public class ShoppingCartContext : DbContext
    {
        public ShoppingCartContext(DbContextOptions<ShoppingCartContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Price).HasPrecision(18, 2);
                entity.HasIndex(p => p.Name);
            });

            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasIndex(c => c.CustomerId);
                entity.HasMany(c => c.Items)
                    .WithOne(i => i.Cart)
                    .HasForeignKey(i => i.CartId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasIndex(ci => new { ci.CartId, ci.ProductId }).IsUnique();
            });
        }
    }
}
