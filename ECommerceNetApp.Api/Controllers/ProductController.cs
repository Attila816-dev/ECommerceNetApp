using ECommerceNetApp.Api.Model;
using ECommerceNetApp.Api.Services;
using ECommerceNetApp.Service.Commands.Product;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces;
using ECommerceNetApp.Service.Queries.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceNetApp.Api.Controllers
{
    /// <summary>
    /// Controller for managing products in the E-commerce application.
    /// </summary>
    [Route("api/products")]
    public class ProductController(IDispatcher dispatcher, IHateoasLinkService linkService)
        : BaseApiController(linkService, dispatcher)
    {
        /// <summary>
        /// Retrieves all products.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>A list of all products.</returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<CollectionLinkedResourceDto<ProductDto>>> GetAllProducts(CancellationToken cancellationToken)
        {
            var products = await Dispatcher.SendQueryAsync<GetAllProductsQuery, IEnumerable<ProductDto>>(new GetAllProductsQuery(), cancellationToken).ConfigureAwait(false);

            var links = new List<LinkDto>
            {
                LinkService.CreateLink(this, nameof(GetAllProducts), rel: "self"),
                LinkService.CreateLink(this, nameof(CreateProduct), rel: "create_product", method: "POST"),
                LinkService.CreateLink(this, nameof(GetPaginatedProducts), rel: "paginated_products"),
            };

            return Ok(CreateCollectionResource(products, links));
        }

        /// <summary>
        /// Retrieves products by category ID.
        /// </summary>
        /// <param name="categoryId">The ID of the category.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>A list of products in the specified category.</returns>
        [HttpGet("by-category")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<CollectionLinkedResourceDto<ProductDto>>> GetProductsByCategoryId(
            [FromQuery] int categoryId,
            CancellationToken cancellationToken)
        {
            var query = new GetProductsByCategoryQuery(categoryId);
            var products = await Dispatcher.SendQueryAsync<GetProductsByCategoryQuery, IEnumerable<ProductDto>>(query, cancellationToken).ConfigureAwait(false);

            var links = new List<LinkDto>
            {
                LinkService.CreateLink(this, nameof(GetProductsByCategoryId), values: new { categoryId }, rel: "self"),
                LinkService.CreateLink(this, nameof(GetAllProducts), rel: "all_products"),
                LinkService.CreateLink(this, nameof(GetPaginatedProducts), values: new { categoryId }, rel: "paginated"),
                LinkService.CreateLink(this, "GetCategoryById", controllerName: "Category", values: new { id = categoryId }, rel: "category"),
            };

            return Ok(CreateCollectionResource(products, links));
        }

        /// <summary>
        /// Retrieves a product by its ID.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>The product with the specified ID.</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LinkedResourceDto<ProductDto>>> GetProductById(
            int id,
            CancellationToken cancellationToken)
        {
            var query = new GetProductByIdQuery(id);
            var product = await Dispatcher.SendQueryAsync<GetProductByIdQuery, ProductDto?>(query, cancellationToken).ConfigureAwait(false);

            if (product == null)
            {
                return NotFound();
            }

            var links = CreateResourceLinks(id, nameof(GetProductById), nameof(UpdateProduct), nameof(DeleteProduct));

            // Add additional product-specific links
            var getCategoryProductsLink = LinkService.CreateLink(
                this,
                "GetCategoryById",
                controllerName: "Category",
                values: new { id = product.CategoryId },
                rel: "category");

            links = links.Append(getCategoryProductsLink);
            return Ok(CreateResource(product, links));
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
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResourceDto<ProductDto>>> GetPaginatedProducts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? categoryId = null,
            CancellationToken cancellationToken = default)
        {
            var query = new GetPaginatedProductsQuery(pageNumber, pageSize, categoryId);
            var result = await Dispatcher.SendQueryAsync<GetPaginatedProductsQuery, PaginationResult<ProductDto>>(query, cancellationToken).ConfigureAwait(false);

            var totalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize);

            // Create pagination links
            var additionalValues = categoryId.HasValue ? new { categoryId } : null;
            var links = LinkService.CreatePaginationLinks(
                this,
                nameof(GetPaginatedProducts),
                pageNumber,
                pageSize,
                totalPages,
                additionalValues);

            // Add additional resource links
            links.Add(LinkService.CreateLink(this, nameof(GetAllProducts), rel: "all_products"));

            if (categoryId.HasValue)
            {
                var getCategoryLink = LinkService.CreateLink(
                    this,
                    "GetCategoryById",
                    controllerName: "Category",
                    values: new { id = categoryId.Value },
                    rel: "category");
                links.Add(getCategoryLink);
            }

            return Ok(CreatePagedResource(result.Items, pageNumber, pageSize, result.TotalCount, links));
        }

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="productDto">The product data to create.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>The ID of the created product.</returns>
        [HttpPost]
        [Authorize(Policy = "RequireProductManagerRole")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LinkedResourceDto<int>>> CreateProduct(
            [FromBody] CreateProductDto productDto,
            CancellationToken cancellationToken)
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
                productDto.Currency,
                productDto.Amount);

            var createdProductId = await Dispatcher.SendCommandAsync<CreateProductCommand, int>(command, cancellationToken).ConfigureAwait(false);

            var getProductLink = LinkService.CreateLink(
                this,
                nameof(GetProductById),
                values: new { id = createdProductId },
                rel: "self");

            var links = new List<LinkDto>
            {
                getProductLink,
            };

            var resource = CreateResource(createdProductId, links);

            return CreatedAtAction(
                nameof(GetProductById),
                new { id = createdProductId },
                resource);
        }

        /// <summary>
        /// Updates an existing product.
        /// </summary>
        /// <param name="id">The ID of the product to update.</param>
        /// <param name="productDto">The updated product data.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>No content if the update is successful.</returns>
        [HttpPut("{id}")]
        [Authorize(Policy = "RequireProductManagerRole")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct(
            int id,
            [FromBody] UpdateProductDto productDto,
            CancellationToken cancellationToken)
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
                productDto.Currency,
                productDto.Amount);

            await Dispatcher.SendCommandAsync<UpdateProductCommand>(command, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }

        /// <summary>
        /// Deletes a product by its ID.
        /// </summary>
        /// <param name="id">The ID of the product to delete.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>No content if the deletion is successful.</returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireProductManagerRole")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
        {
            await Dispatcher.SendCommandAsync<DeleteProductCommand>(new DeleteProductCommand(id), cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
    }
}