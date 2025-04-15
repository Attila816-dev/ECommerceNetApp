using ECommerceNetApp.Service.Commands.Product;
using ECommerceNetApp.Service.DTO;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts(CancellationToken cancellationToken)
        {
            var products = await _mediator.Send(new GetAllProductsQuery(), cancellationToken).ConfigureAwait(false);
            return Ok(products);
        }

        [HttpGet("GetProductsByCategoryId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategoryId([FromQuery] int categoryId, CancellationToken cancellationToken)
        {
            var query = new GetProductsByCategoryQuery(categoryId);
            var products = await _mediator.Send(query, cancellationToken).ConfigureAwait(false);
            return Ok(products);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> GetProductById(int id, CancellationToken cancellationToken)
        {
            var query = new GetProductByIdQuery(id);
            var product = await _mediator.Send(query, cancellationToken).ConfigureAwait(false);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProduct([FromBody] ProductDto productDto, CancellationToken cancellationToken)
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
            return Created();
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDto productDto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(productDto);
            if (id != productDto.Id)
            {
                return BadRequest("The ID in the URL does not match the ID in the request body.");
            }

            var command = new UpdateProductCommand(
                id,
                productDto.Name,
                productDto.Description,
                productDto.ImageUrl,
                productDto.CategoryId,
                productDto.Price,
                productDto.Amount);
            await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
            return Ok();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeleteProductCommand(id), cancellationToken).ConfigureAwait(false);
            return Ok();
        }
    }
}
