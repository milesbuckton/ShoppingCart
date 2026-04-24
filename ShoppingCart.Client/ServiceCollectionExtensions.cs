using Microsoft.Extensions.DependencyInjection;

namespace ShoppingCart.Client
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddShoppingCartClient(this IServiceCollection services, Action<ShoppingCartClientOptions> configureOptions)
        {
            ShoppingCartClientOptions options = new();
            configureOptions(options);

            services.AddHttpClient<IProductsClient, ProductsClient>(client =>
            {
                client.BaseAddress = new Uri(options.BaseAddress);
            });

            services.AddHttpClient<ICartsClient, CartsClient>(client =>
            {
                client.BaseAddress = new Uri(options.BaseAddress);
            });

            return services;
        }
    }
}
