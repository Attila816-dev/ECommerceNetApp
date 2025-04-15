using System.Threading;
using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Service.Commands.Category;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces;
using ECommerceNetApp.Service.Queries.Cart;
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
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAllCategories()
        {
            var categories = await _mediator.Send(new GetAllCategoriesQuery()).ConfigureAwait(false);
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategoryById(int id, CancellationToken cancellationToken)
        {
            var category = await _mediator.Send(new GetCategoryByIdQuery(id), cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CategoryDto categoryDto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(categoryDto);
            var command = new CreateCategoryCommand(categoryDto.Name, categoryDto.ImageUrl, categoryDto.ParentCategoryId);
            await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCategory(int id, CategoryDto categoryDto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(categoryDto);
            if (id != categoryDto.Id)
            {
                return BadRequest();
            }

            ArgumentNullException.ThrowIfNull(categoryDto);
            var command = new UpdateCategoryCommand(categoryDto.Id, categoryDto.Name, categoryDto.ImageUrl, categoryDto.ParentCategoryId);
            await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCategory(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.Send(new DeleteCategoryCommand(id), cancellationToken).ConfigureAwait(false);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
