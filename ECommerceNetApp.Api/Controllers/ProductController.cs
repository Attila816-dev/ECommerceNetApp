using ECommerceNetApp.Service.Commands.Product;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces;
using ECommerceNetApp.Service.Queries.Product;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceNetApp.Api.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts(CancellationToken cancellationToken)
        {
            var products = await _mediator.Send(new GetAllProductsQuery(), cancellationToken).ConfigureAwait(false);
            return Ok(products);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(int categoryId, CancellationToken cancellationToken)
        {
            var products = await _mediator.Send(new GetProductsByCategoryQuery(categoryId), cancellationToken).ConfigureAwait(false);
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProductById(int id, CancellationToken cancellationToken)
        {
            var product = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken).ConfigureAwait(false);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct(ProductDto productDto, CancellationToken cancellationToken)
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
                var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProduct(int id, ProductDto productDto, CancellationToken cancellationToken)
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
                await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
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
        public async Task<ActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.Send(new DeleteProductCommand(id), cancellationToken).ConfigureAwait(false);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
