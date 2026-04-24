using Microsoft.EntityFrameworkCore;
using ShoppingCart.Api.Data;
using ShoppingCart.Api.DTOs;
using ShoppingCart.Api.Models;

namespace ShoppingCart.Api.Services;

public class ProductService : IProductService
{
    private readonly ShoppingCartContext _context;

    public ProductService(ShoppingCartContext context)
    {
        _context = context;
    }

    public async Task<List<ProductResponse>> GetAllAsync()
    {
        return await _context.Products
            .Select(p => new ProductResponse(p.Id, p.Name, p.Description, p.Price, p.StockQuantity))
            .ToListAsync();
    }

    public async Task<ProductResponse?> GetByIdAsync(int id)
    {
        Product? product = await _context.Products.FindAsync(id);
        if (product is null)
            return null;

        return new ProductResponse(product.Id, product.Name, product.Description, product.Price, product.StockQuantity);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        Product product = new()
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return new ProductResponse(product.Id, product.Name, product.Description, product.Price, product.StockQuantity);
    }

    public async Task<ProductResponse?> UpdateAsync(int id, UpdateProductRequest request)
    {
        Product? product = await _context.Products.FindAsync(id);
        if (product is null)
            return null;

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;

        await _context.SaveChangesAsync();

        return new ProductResponse(product.Id, product.Name, product.Description, product.Price, product.StockQuantity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Product? product = await _context.Products.FindAsync(id);
        if (product is null)
            return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }
}
