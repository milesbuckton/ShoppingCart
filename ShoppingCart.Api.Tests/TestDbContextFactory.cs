using Microsoft.EntityFrameworkCore;
using ShoppingCart.Api.Data;

namespace ShoppingCart.Api.Tests
{
    public static class TestDbContextFactory
    {
        public static ShoppingCartContext Create(string? dbName = null)
        {
            DbContextOptions<ShoppingCartContext> options = new DbContextOptionsBuilder<ShoppingCartContext>()
                .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
                .Options;

            ShoppingCartContext context = new(options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
