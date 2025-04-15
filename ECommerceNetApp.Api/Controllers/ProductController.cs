using ECommerceNetApp.Service.Commands.Product;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces;
using ECommerceNetApp.Service.Queries.Product;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceNetApp.Api.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync(new GetAllProductsQuery()).ConfigureAwait(false);
            return Ok(products);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(int categoryId)
        {
            var products = await _productService.GetProductsByCategoryIdAsync(new GetProductsByCategoryQuery(categoryId)).ConfigureAwait(false);
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(new GetProductByIdQuery(id)).ConfigureAwait(false);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct(ProductDto productDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(productDto);
                var command = new CreateProductCommand(
                    productDto.Name,
                    productDto.Description,
                    productDto.ImageUrl,
                    productDto.CategoryId,
                    productDto.Price,
                    productDto.Amount);
                var result = await _productService.AddProductAsync(command).ConfigureAwait(false);
                return CreatedAtAction(nameof(GetProductById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProduct(int id, ProductDto productDto)
        {
            ArgumentNullException.ThrowIfNull(productDto);
            if (id != productDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var command = new UpdateProductCommand(
                    id,
                    productDto.Name,
                    productDto.Description,
                    productDto.ImageUrl,
                    productDto.CategoryId,
                    productDto.Price,
                    productDto.Amount);
                await _productService.UpdateProductAsync(command).ConfigureAwait(false);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            try
            {
                var command = new DeleteProductCommand(id);
                await _productService.DeleteProductAsync(command).ConfigureAwait(false);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
