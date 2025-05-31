using ECommerceNetApp.Api.Authorization;
using ECommerceNetApp.Api.Model;
using ECommerceNetApp.Api.Services;
using ECommerceNetApp.Domain.Authorization;
using ECommerceNetApp.Service.Commands.Category;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces;
using ECommerceNetApp.Service.Queries.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceNetApp.Api.Controllers
{
    /// <summary>
    /// Controller for managing categories in the E-commerce application.
    /// </summary>
    [Route("api/categories")]
    public class CategoryController(IHateoasLinkService linkService, IDispatcher dispatcher)
        : BaseApiController(linkService, dispatcher)
    {
        /// <summary>
        /// Retrieves all categories.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>A list of all categories.</returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<CollectionLinkedResourceDto<CategoryDto>>> GetAllCategories(CancellationToken cancellationToken)
        {
            var categories = await Dispatcher.SendQueryAsync<GetAllCategoriesQuery, IEnumerable<CategoryDto>>(new GetAllCategoriesQuery(), cancellationToken).ConfigureAwait(false);

            var links = new List<LinkDto>
            {
                LinkService.CreateLink(this, nameof(GetAllCategories), rel: "self"),
                LinkService.CreateLink(this, nameof(CreateCategory), rel: "create_category", method: "POST"),
            };

            return Ok(CreateCollectionResource(categories, links));
        }

        /// <summary>
        /// Retrieves categories by parent category ID.
        /// </summary>
        /// <param name="parentCategoryId">The ID of the parent category.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>A list of categories under the specified parent category.</returns>
        [HttpGet("by-parent")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<CollectionLinkedResourceDto<CategoryDto>>> GetCategoriesByParentId(
            [FromQuery] int? parentCategoryId,
            CancellationToken cancellationToken)
        {
            var query = new GetCategoriesByParentCategoryIdQuery(parentCategoryId);
            var categories = await Dispatcher.SendQueryAsync<GetCategoriesByParentCategoryIdQuery, IEnumerable<CategoryDto>>(query, cancellationToken).ConfigureAwait(false);

            var links = new List<LinkDto>
            {
                LinkService.CreateLink(this, nameof(GetCategoriesByParentId), values: new { parentCategoryId }, rel: "self"),
                LinkService.CreateLink(this, nameof(GetAllCategories), rel: "all_categories"),
            };

            if (parentCategoryId.HasValue)
            {
                var parentCategoryLink = LinkService.CreateLink(
                    this,
                    nameof(GetCategoryById),
                    values: new { id = parentCategoryId.Value },
                    rel: "parent_category");
                links.Add(parentCategoryLink);
            }

            return Ok(CreateCollectionResource(categories, links));
        }

        /// <summary>
        /// Retrieves a category by its ID.
        /// </summary>
        /// <param name="id">The ID of the category.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>The category with the specified ID.</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LinkedResourceDto<CategoryDetailDto>>> GetCategoryById(
            int id,
            CancellationToken cancellationToken)
        {
            var category = await Dispatcher.SendQueryAsync<GetCategoryByIdQuery, CategoryDetailDto?>(new GetCategoryByIdQuery(id), cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                return NotFound();
            }

            var links = CreateResourceLinks(id, nameof(GetCategoryById), nameof(UpdateCategory), nameof(DeleteCategory));

            // Add additional category-specific links
            var subCategoriesLink = LinkService.CreateLink(
                this,
                nameof(GetCategoriesByParentId),
                values: new { parentCategoryId = id },
                rel: "subcategories");

            var productsLink = LinkService.CreateLink(
                this,
                nameof(ProductController.GetProductsByCategoryId),
                controllerName: "Product",
                values: new { categoryId = id },
                rel: "products");

            links = links.Concat(
            [
                subCategoriesLink,
                productsLink,
            ]);

            // Add parent category link if available
            if (category.ParentCategoryId.HasValue)
            {
                var parentCategoryLink = LinkService.CreateLink(
                    this,
                    nameof(GetCategoryById),
                    values: new { id = category.ParentCategoryId },
                    rel: "parent_category");
                links = links.Append(parentCategoryLink);
            }

            return Ok(CreateResource(category, links));
        }

        /// <summary>
        /// Creates a new category.
        /// </summary>
        /// <param name="categoryDto">The category data to create.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>The ID of the created category.</returns>
        [HttpPost]
        [RequirePermission(Permissions.Create, Resources.Category)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LinkedResourceDto<int>>> CreateCategory(
            [FromBody] CreateCategoryDto categoryDto,
            CancellationToken cancellationToken)
        {
            if (categoryDto == null)
            {
                return BadRequest("Category data is required.");
            }

            var command = new CreateCategoryCommand(
                categoryDto.Name,
                categoryDto.ImageUrl,
                categoryDto.ParentCategoryId);

            var createdCategoryId = await Dispatcher.SendCommandAsync<CreateCategoryCommand, int>(command, cancellationToken).ConfigureAwait(false);

            var categoryLink = LinkService.CreateLink(
                this,
                nameof(GetCategoryById),
                values: new { id = createdCategoryId },
                rel: "self");

            var links = new List<LinkDto>
            {
                categoryLink,
            };

            var resource = CreateResource(createdCategoryId, links);

            return CreatedAtAction(
                nameof(GetCategoryById),
                new { id = createdCategoryId },
                resource);
        }

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        /// <param name="id">The ID of the category to update.</param>
        /// <param name="categoryDto">The updated category data.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>No content if the update is successful.</returns>
        [HttpPut("{id}")]
        [RequirePermission(Permissions.Update, Resources.Category)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(
            int id,
            [FromBody] UpdateCategoryDto categoryDto,
            CancellationToken cancellationToken)
        {
            if (categoryDto == null)
            {
                return BadRequest("Category data is required.");
            }

            if (id != categoryDto.Id)
            {
                return BadRequest("Invalid category data.");
            }

            var command = new UpdateCategoryCommand(
                categoryDto.Id,
                categoryDto.Name,
                categoryDto.ImageUrl,
                categoryDto.ParentCategoryId);

            await Dispatcher.SendCommandAsync(command, cancellationToken).ConfigureAwait(false);

            return NoContent();
        }

        /// <summary>
        /// Deletes a category by its ID.
        /// </summary>
        /// <param name="id">The ID of the category to delete.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>OK if the deletion is successful.</returns>
        [HttpDelete("{id}")]
        [RequirePermission(Permissions.Delete, Resources.Category)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken)
        {
            await Dispatcher.SendCommandAsync(new DeleteCategoryCommand(id), cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
    }
}
