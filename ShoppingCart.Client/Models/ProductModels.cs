namespace ShoppingCart.Client.Models
{
    public record CreateProductRequest(string Name, string? Description, decimal Price, int StockQuantity);
    public record UpdateProductRequest(string Name, string? Description, decimal Price, int StockQuantity);
    public record ProductResponse(int Id, string Name, string? Description, decimal Price, int StockQuantity);
}
