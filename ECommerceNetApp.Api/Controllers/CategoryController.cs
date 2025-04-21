using ECommerceNetApp.Service.Commands.Category;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.Category;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceNetApp.Api.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoryController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAllCategories()
        {
            var categories = await _mediator.Send(new GetAllCategoriesQuery()).ConfigureAwait(false);
            return Ok(categories);
        }

        [HttpGet("GetCategoriesByParentCategory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategoriesByParentId([FromQuery] int? parentCategoryId, CancellationToken cancellationToken)
        {
            var query = new GetCategoriesByParentCategoryIdQuery(parentCategoryId);
            var categories = await _mediator.Send(query, cancellationToken).ConfigureAwait(false);
            return Ok(categories);
        }

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

            // Add self link
            linkedResource.AddLink(new LinkDto(
                Url.Action(nameof(GetCategoryById), "Category", new { id }, Request.Scheme)!,
                "self",
                "GET"));

            // Add update link
            linkedResource.AddLink(new LinkDto(
                Url.Action(nameof(UpdateCategory), "Category", new { id }, Request.Scheme)!,
                "update_category",
                "PUT"));

            // Add delete link
            linkedResource.AddLink(new LinkDto(
                Url.Action(nameof(DeleteCategory), "Category", new { id }, Request.Scheme)!,
                "delete_category",
                "DELETE"));

            // Add link to related products
            linkedResource.AddLink(new LinkDto(
                Url.Action(nameof(ProductController.GetProductsByCategoryId), "Product", new { categoryId = id }, Request.Scheme)!,
                "products",
                "GET"));

            return Ok(linkedResource);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> CreateCategory([FromBody] CategoryDto categoryDto, CancellationToken cancellationToken)
        {
            if (categoryDto == null)
            {
                return BadRequest("Category data is required.");
            }

            var command = new CreateCategoryCommand(categoryDto.Name, categoryDto.ImageUrl, categoryDto.ParentCategoryId);
            var createdCategoryId = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);

            return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategoryId }, createdCategoryId);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDto categoryDto, CancellationToken cancellationToken)
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
