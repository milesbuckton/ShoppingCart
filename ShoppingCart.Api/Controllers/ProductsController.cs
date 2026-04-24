using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Api.DTOs;
using ShoppingCart.Api.Services;

namespace ShoppingCart.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductResponse>>> GetAll()
        {
            List<ProductResponse> products = await _productService.GetAllAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductResponse>> GetById(int id)
        {
            ProductResponse? product = await _productService.GetByIdAsync(id);
            if (product is null)
                return NotFound();

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request)
        {
            ProductResponse response = await _productService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProductResponse>> Update(int id, UpdateProductRequest request)
        {
            ProductResponse? response = await _productService.UpdateAsync(id, request);
            if (response is null)
                return NotFound();

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _productService.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
