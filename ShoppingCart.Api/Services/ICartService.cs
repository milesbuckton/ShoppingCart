using ShoppingCart.Api.DTOs;

namespace ShoppingCart.Api.Services;

public interface ICartService
{
    Task<CartResponse?> GetCartAsync(Guid customerId);
    Task<ServiceResult<CartResponse>> AddItemAsync(Guid customerId, AddToCartRequest request);
    Task<ServiceResult<CartResponse>> UpdateItemAsync(Guid customerId, int itemId, UpdateCartItemRequest request);
    Task<ServiceResult<CartResponse>> RemoveItemAsync(Guid customerId, int itemId);
    Task<bool> ClearCartAsync(Guid customerId);
}
