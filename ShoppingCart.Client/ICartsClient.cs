using ShoppingCart.Client.Models;

namespace ShoppingCart.Client;

public interface ICartsClient
{
    Task<CartResponse?> GetCartAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<CartResponse> AddItemAsync(Guid customerId, AddToCartRequest request, CancellationToken cancellationToken = default);
    Task<CartResponse> UpdateItemAsync(Guid customerId, int itemId, UpdateCartItemRequest request, CancellationToken cancellationToken = default);
    Task<CartResponse> RemoveItemAsync(Guid customerId, int itemId, CancellationToken cancellationToken = default);
    Task ClearCartAsync(Guid customerId, CancellationToken cancellationToken = default);
}
