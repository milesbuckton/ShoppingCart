using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Api.DTOs;
using ShoppingCart.Api.Services;

namespace ShoppingCart.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartsController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartsController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("{customerId:guid}")]
        public async Task<ActionResult<CartResponse>> GetCart(Guid customerId)
        {
            CartResponse? cart = await _cartService.GetCartAsync(customerId);
            if (cart is null)
                return NotFound();

            return Ok(cart);
        }

        [HttpPost("{customerId:guid}/items")]
        public async Task<ActionResult<CartResponse>> AddItem(Guid customerId, AddToCartRequest request)
        {
            ServiceResult<CartResponse> result = await _cartService.AddItemAsync(customerId, request);
            return ToActionResult(result);
        }

        [HttpPut("{customerId:guid}/items/{itemId}")]
        public async Task<ActionResult<CartResponse>> UpdateItem(Guid customerId, int itemId, UpdateCartItemRequest request)
        {
            ServiceResult<CartResponse> result = await _cartService.UpdateItemAsync(customerId, itemId, request);
            return ToActionResult(result);
        }

        [HttpDelete("{customerId:guid}/items/{itemId}")]
        public async Task<ActionResult<CartResponse>> RemoveItem(Guid customerId, int itemId)
        {
            ServiceResult<CartResponse> result = await _cartService.RemoveItemAsync(customerId, itemId);
            return ToActionResult(result);
        }

        [HttpDelete("{customerId:guid}")]
        public async Task<IActionResult> ClearCart(Guid customerId)
        {
            bool cleared = await _cartService.ClearCartAsync(customerId);
            if (!cleared)
                return NotFound();

            return NoContent();
        }

        private ActionResult<T> ToActionResult<T>(ServiceResult<T> result)
        {
            if (result.IsSuccess)
                return Ok(result.Value);

            return result.ErrorType switch
            {
                ServiceErrorType.NotFound => result.Error is not null ? NotFound(result.Error) : NotFound(),
                ServiceErrorType.Validation => BadRequest(result.Error),
                _ => StatusCode(500)
            };
        }
    }
}
