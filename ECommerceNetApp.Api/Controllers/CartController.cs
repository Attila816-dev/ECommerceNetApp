using ECommerceNetApp.Service.Commands.Cart;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.Cart;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceNetApp.Api.Controllers
{
    [ApiController]
    [Route("api/carts")]
    public class CartController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CartController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet("{cartId}/items")]
        public async Task<ActionResult<List<CartItemDto>>> GetCartItems(string cartId, CancellationToken cancellationToken)
        {
            var cartItems = await _mediator.Send(new GetCartItemsQuery(cartId), cancellationToken).ConfigureAwait(false);
            if (cartItems == null)
            {
                return NotFound();
            }

            return Ok(cartItems);
        }

        [HttpPost("{cartId}/items")]
        public async Task<ActionResult> AddItemToCart(string cartId, CartItemDto item, CancellationToken cancellationToken)
        {
            await _mediator.Send(new AddCartItemCommand(cartId, item), cancellationToken).ConfigureAwait(false);
            return Ok();
        }

        [HttpDelete("{cartId}/items/{itemId}")]
        public async Task<ActionResult> RemoveItemFromCart(string cartId, int itemId, CancellationToken cancellationToken)
        {
            await _mediator.Send(new RemoveCartItemCommand(cartId, itemId), cancellationToken).ConfigureAwait(false);
            return Ok();
        }

        [HttpPut("{cartId}/items/{itemId}")]
        public async Task<ActionResult> UpdateItemQuantity(string cartId, int itemId, [FromBody] int quantity, CancellationToken cancellationToken)
        {
            await _mediator.Send(new UpdateCartItemQuantityCommand(cartId, itemId, quantity), cancellationToken).ConfigureAwait(false);
            return Ok();
        }

        [HttpGet("{cartId}/total")]
        public async Task<ActionResult<decimal>> GetCartTotal(string cartId, CancellationToken cancellationToken)
        {
            var total = await _mediator.Send(new GetCartTotalQuery(cartId), cancellationToken).ConfigureAwait(false);
            if (total == null)
            {
                return NotFound();
            }

            return Ok(total);
        }
    }
}
