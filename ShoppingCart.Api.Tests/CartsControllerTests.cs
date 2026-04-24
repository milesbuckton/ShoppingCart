using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Api.Controllers;
using ShoppingCart.Api.Data;
using ShoppingCart.Api.DTOs;
using ShoppingCart.Api.Models;
using ShoppingCart.Api.Services;

namespace ShoppingCart.Api.Tests
{
    public class CartsControllerTests
    {
        private static readonly Guid CustomerId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        private static (CartsController Controller, ShoppingCartContext Context) CreateController()
        {
            ShoppingCartContext context = TestDbContextFactory.Create();
            CartService service = new(context);
            CartsController controller = new(service);
            return (controller, context);
        }

        private static Product SeedProduct(ShoppingCartContext context, string name = "Test Product", decimal price = 10m, int stock = 100)
        {
            Product product = new() { Name = name, Price = price, StockQuantity = stock };
            context.Products.Add(product);
            context.SaveChanges();
            return product;
        }

        [Fact]
        public async Task GetCart_ReturnsNotFound_WhenNoCart()
        {
            (CartsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                ActionResult<CartResponse> result = await controller.GetCart(CustomerId);

                Assert.IsType<NotFoundResult>(result.Result);
            }
        }

        [Fact]
        public async Task AddItem_CreatesCartAndAddsItem()
        {
            (CartsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                Product product = SeedProduct(context);

                ActionResult<CartResponse> result = await controller.AddItem(CustomerId, new AddToCartRequest(product.Id, 2));

                OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
                CartResponse cart = Assert.IsType<CartResponse>(ok.Value);
                Assert.Equal(CustomerId, cart.CustomerId);
                Assert.Single(cart.Items);
                Assert.Equal(2, cart.Items[0].Quantity);
                Assert.Equal(20m, cart.Total);
            }
        }

        [Fact]
        public async Task AddItem_IncrementsQuantity_WhenProductAlreadyInCart()
        {
            (CartsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                Product product = SeedProduct(context);

                await controller.AddItem(CustomerId, new AddToCartRequest(product.Id, 2));
                ActionResult<CartResponse> result = await controller.AddItem(CustomerId, new AddToCartRequest(product.Id, 3));

                OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
                CartResponse cart = Assert.IsType<CartResponse>(ok.Value);
                Assert.Single(cart.Items);
                Assert.Equal(5, cart.Items[0].Quantity);
            }
        }

        [Fact]
        public async Task AddItem_ReturnsBadRequest_WhenQuantityIsZero()
        {
            (CartsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                Product product = SeedProduct(context);

                ActionResult<CartResponse> result = await controller.AddItem(CustomerId, new AddToCartRequest(product.Id, 0));

                Assert.IsType<BadRequestObjectResult>(result.Result);
            }
        }

        [Fact]
        public async Task AddItem_ReturnsBadRequest_WhenProductNotFound()
        {
            (CartsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                ActionResult<CartResponse> result = await controller.AddItem(CustomerId, new AddToCartRequest(999, 1));

                Assert.IsType<BadRequestObjectResult>(result.Result);
            }
        }

        [Fact]
        public async Task AddItem_ReturnsBadRequest_WhenInsufficientStock()
        {
            (CartsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                Product product = SeedProduct(context, stock: 2);

                ActionResult<CartResponse> result = await controller.AddItem(CustomerId, new AddToCartRequest(product.Id, 5));

                Assert.IsType<BadRequestObjectResult>(result.Result);
            }
        }

        [Fact]
        public async Task GetCart_ReturnsCartWithItems()
        {
            (CartsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                Product product = SeedProduct(context, price: 7.5m);
                await controller.AddItem(CustomerId, new AddToCartRequest(product.Id, 4));

                ActionResult<CartResponse> result = await controller.GetCart(CustomerId);

                OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
                CartResponse cart = Assert.IsType<CartResponse>(ok.Value);
                Assert.Single(cart.Items);
                Assert.Equal(30m, cart.Total);
            }
        }

        [Fact]
        public async Task UpdateItem_ChangesQuantity()
        {
            (CartsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                Product product = SeedProduct(context);
                ActionResult<CartResponse> addResult = await controller.AddItem(CustomerId, new AddToCartRequest(product.Id, 2));
                CartResponse addedCart = Assert.IsType<CartResponse>(Assert.IsType<OkObjectResult>(addResult.Result).Value);
                int itemId = addedCart.Items[0].Id;

                ActionResult<CartResponse> result = await controller.UpdateItem(CustomerId, itemId, new UpdateCartItemRequest(5));

                OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
                CartResponse cart = Assert.IsType<CartResponse>(ok.Value);
                Assert.Equal(5, cart.Items[0].Quantity);
            }
        }

        [Fact]
        public async Task UpdateItem_ReturnsBadRequest_WhenQuantityIsZero()
        {
            (CartsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                Product product = SeedProduct(context);
                ActionResult<CartResponse> addResult = await controller.AddItem(CustomerId, new AddToCartRequest(product.Id, 1));
                CartResponse addedCart = Assert.IsType<CartResponse>(Assert.IsType<OkObjectResult>(addResult.Result).Value);
                int itemId = addedCart.Items[0].Id;

                ActionResult<CartResponse> result = await controller.UpdateItem(CustomerId, itemId, new UpdateCartItemRequest(0));

                Assert.IsType<BadRequestObjectResult>(result.Result);
            }
        }

        [Fact]
        public async Task UpdateItem_ReturnsNotFound_WhenCartMissing()
        {
            (CartsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                ActionResult<CartResponse> result = await controller.UpdateItem(CustomerId, 1, new UpdateCartItemRequest(5));

                Assert.IsType<NotFoundResult>(result.Result);
            }
        }

        [Fact]
        public async Task UpdateItem_ReturnsNotFound_WhenItemMissing()
        {
            (CartsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                Product product = SeedProduct(context);
                await controller.AddItem(CustomerId, new AddToCartRequest(product.Id, 1));

                ActionResult<CartResponse> result = await controller.UpdateItem(CustomerId, 9999, new UpdateCartItemRequest(5));

                Assert.IsType<NotFoundObjectResult>(result.Result);
            }
        }

        [Fact]
        public async Task RemoveItem_RemovesItemFromCart()
        {
            (CartsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                Product product = SeedProduct(context);
                ActionResult<CartResponse> addResult = await controller.AddItem(CustomerId, new AddToCartRequest(product.Id, 2));
                CartResponse addedCart = Assert.IsType<CartResponse>(Assert.IsType<OkObjectResult>(addResult.Result).Value);
                int itemId = addedCart.Items[0].Id;

                ActionResult<CartResponse> result = await controller.RemoveItem(CustomerId, itemId);

                OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
                CartResponse cart = Assert.IsType<CartResponse>(ok.Value);
                Assert.Empty(cart.Items);
                Assert.Equal(0m, cart.Total);
            }
        }

        [Fact]
        public async Task RemoveItem_ReturnsNotFound_WhenCartMissing()
        {
            (CartsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                ActionResult<CartResponse> result = await controller.RemoveItem(CustomerId, 1);

                Assert.IsType<NotFoundResult>(result.Result);
            }
        }

        [Fact]
        public async Task RemoveItem_ReturnsNotFound_WhenItemMissing()
        {
            (CartsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                Product product = SeedProduct(context);
                await controller.AddItem(CustomerId, new AddToCartRequest(product.Id, 1));

                ActionResult<CartResponse> result = await controller.RemoveItem(CustomerId, 9999);

                Assert.IsType<NotFoundObjectResult>(result.Result);
            }
        }

        [Fact]
        public async Task ClearCart_RemovesEntireCart()
        {
            (CartsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                Product product = SeedProduct(context);
                await controller.AddItem(CustomerId, new AddToCartRequest(product.Id, 1));

                IActionResult result = await controller.ClearCart(CustomerId);

                Assert.IsType<NoContentResult>(result);
                Assert.Empty(context.Carts);
            }
        }

        [Fact]
        public async Task ClearCart_ReturnsNotFound_WhenCartMissing()
        {
            (CartsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                IActionResult result = await controller.ClearCart(CustomerId);

                Assert.IsType<NotFoundResult>(result);
            }
        }
    }
}
