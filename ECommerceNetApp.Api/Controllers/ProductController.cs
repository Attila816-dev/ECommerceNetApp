using ECommerceNetApp.Api.Model;
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
        public async Task<ActionResult<LinkedResourceDto<ProductDto>>> GetProductById(int id, CancellationToken cancellationToken)
        {
            var query = new GetProductByIdQuery(id);
            var product = await _mediator.Send(query, cancellationToken).ConfigureAwait(false);

            if (product == null)
            {
                return NotFound();
            }

            // Create linked resource for HATEOAS
            var linkedResource = new LinkedResourceDto<ProductDto>(product);

            // Add self link
            linkedResource.AddLink(new LinkDto(
                Url.Action(nameof(GetProductById), null, new { id }, Request.Scheme)!,
                "self",
                "GET"));

            // Add update link
            linkedResource.AddLink(new LinkDto(
                Url.Action(nameof(UpdateProduct), null, new { id }, Request.Scheme)!,
                "update_product",
                "PUT"));

            // Add delete link
            linkedResource.AddLink(new LinkDto(
                Url.Action(nameof(DeleteProduct), null, new { id }, Request.Scheme)!,
                "delete_product",
                "DELETE"));

            // Add link to category
            linkedResource.AddLink(new LinkDto(
                Url.Action("GetCategoryById", "Category", new { id = product.CategoryId }, Request.Scheme)!,
                "category",
                "GET"));

            return Ok(linkedResource);
        }

        [HttpGet("paginated")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginationWithHateoas<ProductDto>>> GetPaginatedProducts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? categoryId = null,
            CancellationToken cancellationToken = default)
        {
            var query = new GetPaginatedProductsQuery(pageNumber, pageSize, categoryId);
            var result = await _mediator.Send(query, cancellationToken).ConfigureAwait(false);

            // Add HATEOAS links to response
            var links = new List<LinkDto>
            {
                CreatePaginatedLink(pageNumber, pageSize, categoryId, "self"),
            };

            // Add next page link if available
            if (result.HasNextPage)
            {
                links.Add(CreatePaginatedLink(pageNumber + 1, pageSize, categoryId, "next_page"));
            }

            // Add previous page link if available
            if (result.HasPreviousPage)
            {
                links.Add(CreatePaginatedLink(pageNumber - 1, pageSize, categoryId, "previous_page"));
            }

            // Add link to get all products
            links.Add(new LinkDto(
                Url.Action(nameof(GetAllProducts), null, null, Request.Scheme)!,
                "all_products",
                "GET"));

            // Convert to HATEOAS result
            var hateoasResult = PaginationWithHateoas<ProductDto>.FromPaginationResult(result, links);

            return Ok(hateoasResult);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProduct([FromBody] ProductDto productDto, CancellationToken cancellationToken)
        {
            if (productDto == null)
            {
                return BadRequest("Product data is required.");
            }

            var command = new CreateProductCommand(
                productDto.Name,
                productDto.Description,
                productDto.ImageUrl,
                productDto.CategoryId,
                productDto.Price,
                productDto.Amount);

            var createdProductId = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);

            return CreatedAtAction(nameof(GetProductById), new { id = createdProductId }, createdProductId);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDto productDto, CancellationToken cancellationToken)
        {
            if (productDto == null)
            {
                return BadRequest("Product data is required.");
            }

            if (id != productDto.Id)
            {
                return BadRequest("Invalid product data.");
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

            return NoContent();
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

        private LinkDto CreatePaginatedLink(int pageNumber, int pageSize, int? categoryId, string rel)
        {
            return new LinkDto(
                Url.Action(
                    nameof(GetPaginatedProducts),
                    null,
                    new { pageNumber, pageSize, categoryId },
                    Request.Scheme)!,
                rel,
                "GET");
        }
    }
}