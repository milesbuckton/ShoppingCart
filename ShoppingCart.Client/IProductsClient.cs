using ShoppingCart.Client.Models;

namespace ShoppingCart.Client;

public interface IProductsClient
{
    Task<List<ProductResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
