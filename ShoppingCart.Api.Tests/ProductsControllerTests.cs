using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Api.Controllers;
using ShoppingCart.Api.Data;
using ShoppingCart.Api.DTOs;
using ShoppingCart.Api.Models;
using ShoppingCart.Api.Services;

namespace ShoppingCart.Api.Tests
{
    public class ProductsControllerTests
    {
        private static (ProductsController Controller, ShoppingCartContext Context) CreateController()
        {
            ShoppingCartContext context = TestDbContextFactory.Create();
            ProductService service = new(context);
            ProductsController controller = new(service);
            return (controller, context);
        }

        [Fact]
        public async Task GetAll_ReturnsEmptyList_WhenNoProducts()
        {
            (ProductsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                ActionResult<List<ProductResponse>> result = await controller.GetAll();

                OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
                List<ProductResponse> products = Assert.IsType<List<ProductResponse>>(ok.Value);
                Assert.Empty(products);
            }
        }

        [Fact]
        public async Task GetAll_ReturnsAllProducts()
        {
            (ProductsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                context.Products.AddRange(
                    new Product { Name = "Item A", Price = 10m, StockQuantity = 5 },
                    new Product { Name = "Item B", Price = 20m, StockQuantity = 3 }
                );
                await context.SaveChangesAsync();

                ActionResult<List<ProductResponse>> result = await controller.GetAll();

                OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
                List<ProductResponse> products = Assert.IsType<List<ProductResponse>>(ok.Value);
                Assert.Equal(2, products.Count);
            }
        }

        [Fact]
        public async Task GetById_ReturnsProduct_WhenExists()
        {
            (ProductsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                Product product = new() { Name = "Widget", Price = 9.99m, StockQuantity = 10 };
                context.Products.Add(product);
                await context.SaveChangesAsync();

                ActionResult<ProductResponse> result = await controller.GetById(product.Id);

                OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
                ProductResponse response = Assert.IsType<ProductResponse>(ok.Value);
                Assert.Equal("Widget", response.Name);
                Assert.Equal(9.99m, response.Price);
            }
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenMissing()
        {
            (ProductsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                ActionResult<ProductResponse> result = await controller.GetById(999);

                Assert.IsType<NotFoundResult>(result.Result);
            }
        }

        [Fact]
        public async Task Create_AddsProduct_AndReturnsCreatedAtAction()
        {
            (ProductsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                CreateProductRequest request = new("Gadget", "A cool gadget", 29.99m, 50);

                ActionResult<ProductResponse> result = await controller.Create(request);

                CreatedAtActionResult created = Assert.IsType<CreatedAtActionResult>(result.Result);
                ProductResponse response = Assert.IsType<ProductResponse>(created.Value);
                Assert.Equal("Gadget", response.Name);
                Assert.Equal(29.99m, response.Price);
                Assert.Single(context.Products);
            }
        }

        [Fact]
        public async Task Update_ModifiesProduct_WhenExists()
        {
            (ProductsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                Product product = new() { Name = "Old Name", Price = 5m, StockQuantity = 1 };
                context.Products.Add(product);
                await context.SaveChangesAsync();
                UpdateProductRequest request = new("New Name", "Updated", 15m, 20);

                ActionResult<ProductResponse> result = await controller.Update(product.Id, request);

                OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
                ProductResponse response = Assert.IsType<ProductResponse>(ok.Value);
                Assert.Equal("New Name", response.Name);
                Assert.Equal(15m, response.Price);
                Assert.Equal(20, response.StockQuantity);
            }
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenMissing()
        {
            (ProductsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                UpdateProductRequest request = new("Name", null, 10m, 1);

                ActionResult<ProductResponse> result = await controller.Update(999, request);

                Assert.IsType<NotFoundResult>(result.Result);
            }
        }

        [Fact]
        public async Task Delete_RemovesProduct_WhenExists()
        {
            (ProductsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                Product product = new() { Name = "ToDelete", Price = 1m, StockQuantity = 1 };
                context.Products.Add(product);
                await context.SaveChangesAsync();

                IActionResult result = await controller.Delete(product.Id);

                Assert.IsType<NoContentResult>(result);
                Assert.Empty(context.Products);
            }
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            (ProductsController controller, ShoppingCartContext context) = CreateController();
            await using (context)
            {
                IActionResult result = await controller.Delete(999);

                Assert.IsType<NotFoundResult>(result);
            }
        }
    }
}
