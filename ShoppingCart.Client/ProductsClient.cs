using System.Net.Http.Json;
using ShoppingCart.Client.Models;

namespace ShoppingCart.Client
{
    public class ProductsClient : IProductsClient
    {
        private readonly HttpClient _httpClient;

        public ProductsClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ProductResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            List<ProductResponse>? products = await _httpClient.GetFromJsonAsync<List<ProductResponse>>("api/products", cancellationToken);
            return products ?? [];
        }

        public async Task<ProductResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"api/products/{id}", cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProductResponse>(cancellationToken);
        }

        public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/products", request, cancellationToken);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<ProductResponse>(cancellationToken))!;
        }

        public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/products/{id}", request, cancellationToken);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<ProductResponse>(cancellationToken))!;
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync($"api/products/{id}", cancellationToken);
            response.EnsureSuccessStatusCode();
        }
    }
}
