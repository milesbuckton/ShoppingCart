using Microsoft.EntityFrameworkCore;
using ShoppingCart.Api.Data;
using ShoppingCart.Api.DTOs;
using ShoppingCart.Api.Models;

namespace ShoppingCart.Api.Services;

public class CartService : ICartService
{
    private readonly ShoppingCartContext _context;

    public CartService(ShoppingCartContext context)
    {
        _context = context;
    }

    public async Task<CartResponse?> GetCartAsync(Guid customerId)
    {
        Cart? cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        if (cart is null)
            return null;

        return MapToResponse(cart);
    }

    public async Task<ServiceResult<CartResponse>> AddItemAsync(Guid customerId, AddToCartRequest request)
    {
        if (request.Quantity <= 0)
            return ServiceResult<CartResponse>.ValidationError("Quantity must be greater than zero.");

        Product? product = await _context.Products.FindAsync(request.ProductId);
        if (product is null)
            return ServiceResult<CartResponse>.ValidationError("Product not found.");

        if (product.StockQuantity < request.Quantity)
            return ServiceResult<CartResponse>.ValidationError("Insufficient stock.");

        Cart? cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        if (cart is null)
        {
            cart = new Cart() { CustomerId = customerId };
            _context.Carts.Add(cart);
        }

        CartItem? existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (existingItem is not null)
        {
            existingItem.Quantity += request.Quantity;
        }
        else
        {
            cart.Items.Add(new CartItem()
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity
            });
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _context.Entry(cart).Collection(c => c.Items).Query().Include(i => i.Product).LoadAsync();

        return ServiceResult<CartResponse>.Success(MapToResponse(cart));
    }

    public async Task<ServiceResult<CartResponse>> UpdateItemAsync(Guid customerId, int itemId, UpdateCartItemRequest request)
    {
        if (request.Quantity <= 0)
            return ServiceResult<CartResponse>.ValidationError("Quantity must be greater than zero.");

        Cart? cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        if (cart is null)
            return ServiceResult<CartResponse>.NotFound();

        CartItem? item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
            return ServiceResult<CartResponse>.NotFound("Cart item not found.");

        if (item.Product.StockQuantity < request.Quantity)
            return ServiceResult<CartResponse>.ValidationError("Insufficient stock.");

        item.Quantity = request.Quantity;
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ServiceResult<CartResponse>.Success(MapToResponse(cart));
    }

    public async Task<ServiceResult<CartResponse>> RemoveItemAsync(Guid customerId, int itemId)
    {
        Cart? cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        if (cart is null)
            return ServiceResult<CartResponse>.NotFound();

        CartItem? item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
            return ServiceResult<CartResponse>.NotFound("Cart item not found.");

        cart.Items.Remove(item);
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ServiceResult<CartResponse>.Success(MapToResponse(cart));
    }

    public async Task<bool> ClearCartAsync(Guid customerId)
    {
        Cart? cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        if (cart is null)
            return false;

        _context.Carts.Remove(cart);
        await _context.SaveChangesAsync();
        return true;
    }

    private static CartResponse MapToResponse(Cart cart)
    {
        List<CartItemResponse> items = cart.Items.Select(i => new CartItemResponse(
            i.Id,
            i.ProductId,
            i.Product.Name,
            i.Product.Price,
            i.Quantity,
            i.Product.Price * i.Quantity
        )).ToList();

        return new CartResponse(
            cart.Id,
            cart.CustomerId,
            items,
            items.Sum(i => i.Subtotal),
            cart.CreatedAt,
            cart.UpdatedAt
        );
    }
}
