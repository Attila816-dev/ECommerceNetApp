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

        [HttpGet("SubCategories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetSubCategories([FromQuery] int? parentCategoryId, CancellationToken cancellationToken)
        {
            var query = new GetCategoriesByParentCategoryIdQuery(parentCategoryId);
            var categories = await _mediator.Send(query, cancellationToken).ConfigureAwait(false);
            return Ok(categories);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoryDetailDto>> GetCategoryById(int id, CancellationToken cancellationToken)
        {
            var category = await _mediator.Send(new GetCategoryByIdQuery(id), cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> CreateCategory([FromBody] CategoryDto categoryDto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(categoryDto);
            var command = new CreateCategoryCommand(categoryDto.Name, categoryDto.ImageUrl, categoryDto.ParentCategoryId);
            var createdCategoryId = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
            return Created();

            // return CreatedAtAction(nameof(GetCategory), new { id = result }, result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDto categoryDto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(categoryDto);
            if (id != categoryDto.Id)
            {
                return BadRequest("The ID in the URL does not match the ID in the request body.");
            }

            ArgumentNullException.ThrowIfNull(categoryDto);
            var command = new UpdateCategoryCommand(categoryDto.Id, categoryDto.Name, categoryDto.ImageUrl, categoryDto.ParentCategoryId);
            await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
            return Ok();
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
