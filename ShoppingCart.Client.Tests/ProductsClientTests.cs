using System.Net;
using ShoppingCart.Client.Models;

namespace ShoppingCart.Client.Tests
{
    public class ProductsClientTests
    {
        private readonly MockHttpMessageHandler _handler = new();
        private readonly ProductsClient _client;

        public ProductsClientTests()
        {
            _client = new ProductsClient(_handler.CreateClient());
        }

        [Fact]
        public async Task GetAllAsync_ReturnsProducts()
        {
            List<ProductResponse> expected =
            [
                new(1, "Widget", "A widget", 9.99m, 100),
                new(2, "Gadget", null, 19.99m, 50)
            ];
            _handler.RespondWith(HttpStatusCode.OK, expected);

            List<ProductResponse> result = await _client.GetAllAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("Widget", result[0].Name);
            Assert.Equal("Gadget", result[1].Name);
            Assert.EndsWith("api/products", _handler.Requests[0].RequestUri!.ToString());
            Assert.Equal(HttpMethod.Get, _handler.Requests[0].Method);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoProducts()
        {
            _handler.RespondWith(HttpStatusCode.OK, new List<ProductResponse>());

            List<ProductResponse> result = await _client.GetAllAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsProduct_WhenFound()
        {
            ProductResponse expected = new(1, "Widget", "A widget", 9.99m, 100);
            _handler.RespondWith(HttpStatusCode.OK, expected);

            ProductResponse? result = await _client.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Widget", result.Name);
            Assert.Equal(9.99m, result.Price);
            Assert.EndsWith("api/products/1", _handler.Requests[0].RequestUri!.ToString());
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            _handler.RespondWith(HttpStatusCode.NotFound);

            ProductResponse? result = await _client.GetByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_SendsPostAndReturnsProduct()
        {
            CreateProductRequest request = new("Widget", "A widget", 9.99m, 100);
            ProductResponse expected = new(1, "Widget", "A widget", 9.99m, 100);
            _handler.RespondWith(HttpStatusCode.Created, expected);

            ProductResponse result = await _client.CreateAsync(request);

            Assert.Equal("Widget", result.Name);
            Assert.Equal(HttpMethod.Post, _handler.Requests[0].Method);
            Assert.EndsWith("api/products", _handler.Requests[0].RequestUri!.ToString());
        }

        [Fact]
        public async Task UpdateAsync_SendsPutAndReturnsProduct()
        {
            UpdateProductRequest request = new("Updated Widget", "Updated", 14.99m, 75);
            ProductResponse expected = new(1, "Updated Widget", "Updated", 14.99m, 75);
            _handler.RespondWith(HttpStatusCode.OK, expected);

            ProductResponse result = await _client.UpdateAsync(1, request);

            Assert.Equal("Updated Widget", result.Name);
            Assert.Equal(14.99m, result.Price);
            Assert.Equal(HttpMethod.Put, _handler.Requests[0].Method);
            Assert.EndsWith("api/products/1", _handler.Requests[0].RequestUri!.ToString());
        }

        [Fact]
        public async Task DeleteAsync_SendsDeleteRequest()
        {
            _handler.RespondWith(HttpStatusCode.NoContent);

            await _client.DeleteAsync(1);

            Assert.Equal(HttpMethod.Delete, _handler.Requests[0].Method);
            Assert.EndsWith("api/products/1", _handler.Requests[0].RequestUri!.ToString());
        }

        [Fact]
        public async Task DeleteAsync_ThrowsOnServerError()
        {
            _handler.RespondWith(HttpStatusCode.InternalServerError);

            await Assert.ThrowsAsync<HttpRequestException>(() => _client.DeleteAsync(1));
        }

        [Fact]
        public async Task CreateAsync_ThrowsOnBadRequest()
        {
            _handler.RespondWith(HttpStatusCode.BadRequest);

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                _client.CreateAsync(new CreateProductRequest("", null, -1, 0)));
        }
    }
}
