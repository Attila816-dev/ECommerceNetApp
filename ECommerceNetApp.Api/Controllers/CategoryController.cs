using ECommerceNetApp.Api.Model;
using ECommerceNetApp.Service.Commands.Category;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.Category;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceNetApp.Api.Controllers
{
    /// <summary>
    /// Controller for managing categories in the E-commerce application.
    /// </summary>
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator instance for handling requests.</param>
        public CategoryController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Retrieves all categories.
        /// </summary>
        /// <returns>A list of all categories.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAllCategories()
        {
            var categories = await _mediator.Send(new GetAllCategoriesQuery()).ConfigureAwait(false);
            return Ok(categories);
        }

        /// <summary>
        /// Retrieves categories by parent category ID.
        /// </summary>
        /// <param name="parentCategoryId">The ID of the parent category.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>A list of categories under the specified parent category.</returns>
        [HttpGet("GetCategoriesByParentCategory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategoriesByParentId([FromQuery] int? parentCategoryId, CancellationToken cancellationToken)
        {
            var query = new GetCategoriesByParentCategoryIdQuery(parentCategoryId);
            var categories = await _mediator.Send(query, cancellationToken).ConfigureAwait(false);
            return Ok(categories);
        }

        /// <summary>
        /// Retrieves a category by its ID.
        /// </summary>
        /// <param name="id">The ID of the category.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>The category with the specified ID.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LinkedResourceDto<CategoryDetailDto>>> GetCategoryById(int id, CancellationToken cancellationToken)
        {
            var category = await _mediator.Send(new GetCategoryByIdQuery(id), cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                return NotFound();
            }

            var linkedResource = new LinkedResourceDto<CategoryDetailDto>(category);

            linkedResource.AddLink(new LinkDto(
                Url.Action(nameof(GetCategoryById), "Category", new { id }, Request.Scheme)!,
                "self",
                "GET"));

            linkedResource.AddLink(new LinkDto(
                Url.Action(nameof(UpdateCategory), "Category", new { id }, Request.Scheme)!,
                "update_category",
                "PUT"));

            linkedResource.AddLink(new LinkDto(
                Url.Action(nameof(DeleteCategory), "Category", new { id }, Request.Scheme)!,
                "delete_category",
                "DELETE"));

            linkedResource.AddLink(new LinkDto(
                Url.Action(nameof(ProductController.GetProductsByCategoryId), "Product", new { categoryId = id }, Request.Scheme)!,
                "products",
                "GET"));

            return Ok(linkedResource);
        }

        /// <summary>
        /// Creates a new category.
        /// </summary>
        /// <param name="categoryDto">The category data to create.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>The ID of the created category.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> CreateCategory([FromBody] CreateCategoryDto categoryDto, CancellationToken cancellationToken)
        {
            if (categoryDto == null)
            {
                return BadRequest("Category data is required.");
            }

            var command = new CreateCategoryCommand(categoryDto.Name, categoryDto.ImageUrl, categoryDto.ParentCategoryId);
            var createdCategoryId = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);

            return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategoryId }, createdCategoryId);
        }

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        /// <param name="id">The ID of the category to update.</param>
        /// <param name="categoryDto">The updated category data.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>No content if the update is successful.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto categoryDto, CancellationToken cancellationToken)
        {
            if (categoryDto == null)
            {
                return BadRequest("Category data is required.");
            }

            if (id != categoryDto.Id)
            {
                return BadRequest("Invalid category data.");
            }

            var command = new UpdateCategoryCommand(categoryDto.Id, categoryDto.Name, categoryDto.ImageUrl, categoryDto.ParentCategoryId);
            await _mediator.Send(command, cancellationToken).ConfigureAwait(false);

            return NoContent();
        }

        /// <summary>
        /// Deletes a category by its ID.
        /// </summary>
        /// <param name="id">The ID of the category to delete.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>OK if the deletion is successful.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeleteCategoryCommand(id), cancellationToken).ConfigureAwait(false);
            return Ok();
        }
    }
}
