using System.Net.Http.Json;
using ShoppingCart.Client.Models;

namespace ShoppingCart.Client
{
    public class CartsClient : ICartsClient
    {
        private readonly HttpClient _httpClient;

        public CartsClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CartResponse?> GetCartAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"api/carts/{customerId}", cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CartResponse>(cancellationToken);
        }

        public async Task<CartResponse> AddItemAsync(Guid customerId, AddToCartRequest request, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/carts/{customerId}/items", request, cancellationToken);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<CartResponse>(cancellationToken))!;
        }

        public async Task<CartResponse> UpdateItemAsync(Guid customerId, int itemId, UpdateCartItemRequest request, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/carts/{customerId}/items/{itemId}", request, cancellationToken);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<CartResponse>(cancellationToken))!;
        }

        public async Task<CartResponse> RemoveItemAsync(Guid customerId, int itemId, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync($"api/carts/{customerId}/items/{itemId}", cancellationToken);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<CartResponse>(cancellationToken))!;
        }

        public async Task ClearCartAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync($"api/carts/{customerId}", cancellationToken);
            response.EnsureSuccessStatusCode();
        }
    }
}
