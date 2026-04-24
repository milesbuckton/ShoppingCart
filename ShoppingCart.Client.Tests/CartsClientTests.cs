using System.Net;
using ShoppingCart.Client.Models;

namespace ShoppingCart.Client.Tests
{
    public class CartsClientTests
    {
        private readonly MockHttpMessageHandler _handler = new();
        private readonly CartsClient _client;
        private readonly Guid _customerId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        public CartsClientTests()
        {
            _client = new CartsClient(_handler.CreateClient());
        }

        private CartResponse CreateSampleCart(params CartItemResponse[] items) => 
            new(1, _customerId, items.ToList(), items.Sum(i => i.Subtotal), DateTime.UtcNow, DateTime.UtcNow);

        [Fact]
        public async Task GetCartAsync_ReturnsCart_WhenFound()
        {
            CartResponse cart = CreateSampleCart(new CartItemResponse(1, 1, "Widget", 9.99m, 2, 19.98m));
            _handler.RespondWith(HttpStatusCode.OK, cart);

            CartResponse? result = await _client.GetCartAsync(_customerId);

            Assert.NotNull(result);
            Assert.Equal(_customerId, result.CustomerId);
            Assert.Single(result.Items);
            Assert.Equal("Widget", result.Items[0].ProductName);
            Assert.Contains(_customerId.ToString(), _handler.Requests[0].RequestUri!.ToString());
            Assert.Equal(HttpMethod.Get, _handler.Requests[0].Method);
        }

        [Fact]
        public async Task GetCartAsync_ReturnsNull_WhenNotFound()
        {
            _handler.RespondWith(HttpStatusCode.NotFound);

            CartResponse? result = await _client.GetCartAsync(_customerId);

            Assert.Null(result);
        }

        [Fact]
        public async Task AddItemAsync_SendsPostAndReturnsCart()
        {
            AddToCartRequest request = new(1, 3);
            CartResponse cart = CreateSampleCart(new CartItemResponse(1, 1, "Widget", 9.99m, 3, 29.97m));
            _handler.RespondWith(HttpStatusCode.OK, cart);

            CartResponse result = await _client.AddItemAsync(_customerId, request);

            Assert.Equal(3, result.Items[0].Quantity);
            Assert.Equal(HttpMethod.Post, _handler.Requests[0].Method);
            Assert.EndsWith("/items", _handler.Requests[0].RequestUri!.ToString());
        }

        [Fact]
        public async Task UpdateItemAsync_SendsPutAndReturnsCart()
        {
            UpdateCartItemRequest request = new(5);
            CartResponse cart = CreateSampleCart(new CartItemResponse(1, 1, "Widget", 9.99m, 5, 49.95m));
            _handler.RespondWith(HttpStatusCode.OK, cart);

            CartResponse result = await _client.UpdateItemAsync(_customerId, 1, request);

            Assert.Equal(5, result.Items[0].Quantity);
            Assert.Equal(HttpMethod.Put, _handler.Requests[0].Method);
            Assert.EndsWith("/items/1", _handler.Requests[0].RequestUri!.ToString());
        }

        [Fact]
        public async Task RemoveItemAsync_SendsDeleteAndReturnsCart()
        {
            CartResponse cart = CreateSampleCart();
            _handler.RespondWith(HttpStatusCode.OK, cart);

            CartResponse result = await _client.RemoveItemAsync(_customerId, 1);

            Assert.Empty(result.Items);
            Assert.Equal(HttpMethod.Delete, _handler.Requests[0].Method);
            Assert.EndsWith("/items/1", _handler.Requests[0].RequestUri!.ToString());
        }

        [Fact]
        public async Task ClearCartAsync_SendsDeleteRequest()
        {
            _handler.RespondWith(HttpStatusCode.NoContent);

            await _client.ClearCartAsync(_customerId);

            Assert.Equal(HttpMethod.Delete, _handler.Requests[0].Method);
            Assert.EndsWith($"api/carts/{_customerId}", _handler.Requests[0].RequestUri!.ToString());
        }

        [Fact]
        public async Task ClearCartAsync_ThrowsOnNotFound()
        {
            _handler.RespondWith(HttpStatusCode.NotFound);

            await Assert.ThrowsAsync<HttpRequestException>(() => _client.ClearCartAsync(_customerId));
        }

        [Fact]
        public async Task AddItemAsync_ThrowsOnBadRequest()
        {
            _handler.RespondWith(HttpStatusCode.BadRequest);

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                _client.AddItemAsync(_customerId, new AddToCartRequest(999, 1)));
        }

        [Fact]
        public async Task GetCartAsync_UsesCorrectUrl()
        {
            _handler.RespondWith(HttpStatusCode.NotFound);

            await _client.GetCartAsync(_customerId);

            Assert.EndsWith($"api/carts/{_customerId}", _handler.Requests[0].RequestUri!.ToString());
        }

        [Fact]
        public async Task AddItemAsync_UsesCorrectUrl()
        {
            CartResponse cart = CreateSampleCart();
            _handler.RespondWith(HttpStatusCode.OK, cart);

            await _client.AddItemAsync(_customerId, new AddToCartRequest(1, 1));

            Assert.EndsWith($"api/carts/{_customerId}/items", _handler.Requests[0].RequestUri!.ToString());
        }
    }
}
