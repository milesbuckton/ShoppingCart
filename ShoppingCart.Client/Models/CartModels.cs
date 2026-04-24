namespace ShoppingCart.Client.Models
{
    public record AddToCartRequest(int ProductId, int Quantity);
    public record UpdateCartItemRequest(int Quantity);
    public record CartItemResponse(int Id, int ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal Subtotal);
    public record CartResponse(int Id, Guid CustomerId, List<CartItemResponse> Items, decimal Total, DateTime CreatedAt, DateTime UpdatedAt);
}
