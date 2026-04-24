using ShoppingCart.Api.DTOs;

namespace ShoppingCart.Api.Services;

public interface IProductService
{
    Task<List<ProductResponse>> GetAllAsync();
    Task<ProductResponse?> GetByIdAsync(int id);
    Task<ProductResponse> CreateAsync(CreateProductRequest request);
    Task<ProductResponse?> UpdateAsync(int id, UpdateProductRequest request);
    Task<bool> DeleteAsync(int id);
}
