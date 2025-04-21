using ECommerceNetApp.Api.Model;
using ECommerceNetApp.Service.Commands.Product;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.Product;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceNetApp.Api.Controllers
{
    /// <summary>
    /// Controller for managing products in the E-commerce application.
    /// </summary>
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator instance for handling requests.</param>
        public ProductController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Retrieves all products.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>A list of all products.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts(CancellationToken cancellationToken)
        {
            var products = await _mediator.Send(new GetAllProductsQuery(), cancellationToken).ConfigureAwait(false);
            return Ok(products);
        }

        /// <summary>
        /// Retrieves products by category ID.
        /// </summary>
        /// <param name="categoryId">The ID of the category.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>A list of products in the specified category.</returns>
        [HttpGet("GetProductsByCategoryId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategoryId([FromQuery] int categoryId, CancellationToken cancellationToken)
        {
            var query = new GetProductsByCategoryQuery(categoryId);
            var products = await _mediator.Send(query, cancellationToken).ConfigureAwait(false);
            return Ok(products);
        }

        /// <summary>
        /// Retrieves a product by its ID.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>The product with the specified ID.</returns>
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

            var linkedResource = new LinkedResourceDto<ProductDto>(product);

            linkedResource.AddLink(new LinkDto(
                Url.Action(nameof(GetProductById), null, new { id }, Request.Scheme)!,
                "self",
                "GET"));

            linkedResource.AddLink(new LinkDto(
                Url.Action(nameof(UpdateProduct), null, new { id }, Request.Scheme)!,
                "update_product",
                "PUT"));

            linkedResource.AddLink(new LinkDto(
                Url.Action(nameof(DeleteProduct), null, new { id }, Request.Scheme)!,
                "delete_product",
                "DELETE"));

            linkedResource.AddLink(new LinkDto(
                Url.Action("GetCategoryById", "Category", new { id = product.CategoryId }, Request.Scheme)!,
                "category",
                "GET"));

            return Ok(linkedResource);
        }

        /// <summary>
        /// Retrieves paginated products with optional filtering by category.
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="categoryId">Optional category ID to filter products.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>A paginated list of products.</returns>
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

            var links = new List<LinkDto>
            {
                CreatePaginatedLink(pageNumber, pageSize, categoryId, "self"),
            };

            if (result.HasNextPage)
            {
                links.Add(CreatePaginatedLink(pageNumber + 1, pageSize, categoryId, "next_page"));
            }

            if (result.HasPreviousPage)
            {
                links.Add(CreatePaginatedLink(pageNumber - 1, pageSize, categoryId, "previous_page"));
            }

            links.Add(new LinkDto(
                Url.Action(nameof(GetAllProducts), null, null, Request.Scheme)!,
                "all_products",
                "GET"));

            var hateoasResult = PaginationWithHateoas<ProductDto>.FromPaginationResult(result, links);

            return Ok(hateoasResult);
        }

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="productDto">The product data to create.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>The ID of the created product.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto productDto, CancellationToken cancellationToken)
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

        /// <summary>
        /// Updates an existing product.
        /// </summary>
        /// <param name="id">The ID of the product to update.</param>
        /// <param name="productDto">The updated product data.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>No content if the update is successful.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto productDto, CancellationToken cancellationToken)
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

        /// <summary>
        /// Deletes a product by its ID.
        /// </summary>
        /// <param name="id">The ID of the product to delete.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>OK if the deletion is successful.</returns>
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
