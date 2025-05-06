using Asp.Versioning;
using ECommerceNetApp.Api.Model;
using ECommerceNetApp.Api.Services;
using ECommerceNetApp.Service.Commands.Cart;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces;
using ECommerceNetApp.Service.Queries.Cart;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceNetApp.Api.Controllers.V1
{
    /// <summary>
    /// Controller for managing shopping carts in the E-commerce application (API Version 1).
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/carts")]
    public class CartController(IDispatcher dispatcher, IHateoasLinkService linkService)
        : BaseApiController(linkService, dispatcher)
    {
        /// <summary>
        /// Retrieves cart information.
        /// </summary>
        /// <param name="cartId">The ID of the cart.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>A cart model with a list of items.</returns>
        [HttpGet("{cartId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LinkedResourceDto<CartDto>>> GetCart(
            string cartId,
            CancellationToken cancellationToken)
        {
            var query = new GetCartQuery(cartId);
            var cart = await Dispatcher.SendQueryAsync<GetCartQuery, CartDto?>(query, cancellationToken).ConfigureAwait(false);

            if (cart == null)
            {
                return NotFound();
            }

            var links = new List<LinkDto>
            {
                LinkService.CreateLink(this, nameof(GetCart), values: new { cartId }, rel: "self"),
                LinkService.CreateLink(this, nameof(AddItemToCart), values: new { cartId }, rel: "add_item", method: "POST"),
                LinkService.CreateLink(this, nameof(GetCartTotal), values: new { cartId }, rel: "cart_total"),
            };

            return Ok(CreateResource(cart, links));
        }

        /// <summary>
        /// Adds an item to a cart.
        /// </summary>
        /// <param name="cartId">The ID of the cart.</param>
        /// <param name="cartItem">The cart item to add.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>The created resource with links.</returns>
        [HttpPost("{cartId}/items")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LinkedResourceDto<string>>> AddItemToCart(
            string cartId,
            [FromBody] CartItemDto cartItem,
            CancellationToken cancellationToken)
        {
            if (cartItem == null)
            {
                return BadRequest("Cart item is required.");
            }

            await Dispatcher.SendCommandAsync(new AddCartItemCommand(cartId, cartItem), cancellationToken).ConfigureAwait(false);

            var links = new List<LinkDto>
            {
                LinkService.CreateLink(this, nameof(GetCart), values: new { cartId }, rel: "cart"),
                LinkService.CreateLink(this, nameof(GetCartTotal), values: new { cartId }, rel: "total"),
            };

            var resource = CreateResource(cartId, links);

            return CreatedAtAction(nameof(GetCart), new { cartId, version = "1.0" }, resource);
        }

        /// <summary>
        /// Removes an item from a cart.
        /// </summary>
        /// <param name="cartId">The ID of the cart.</param>
        /// <param name="itemId">The ID of the item to remove.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>No content if the deletion is successful.</returns>
        [HttpDelete("{cartId}/items/{itemId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RemoveItemFromCart(
            string cartId,
            int itemId,
            CancellationToken cancellationToken)
        {
            await Dispatcher.SendCommandAsync(new RemoveCartItemCommand(cartId, itemId), cancellationToken).ConfigureAwait(false);
            return NoContent();
        }

        /// <summary>
        /// Gets the total value of a cart.
        /// </summary>
        /// <param name="cartId">The ID of the cart.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>The cart total with links.</returns>
        [HttpGet("{cartId}/total")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LinkedResourceDto<decimal>>> GetCartTotal(
            string cartId,
            CancellationToken cancellationToken)
        {
            var total = await Dispatcher.SendQueryAsync<GetCartTotalQuery, decimal?>(new GetCartTotalQuery(cartId), cancellationToken).ConfigureAwait(false);
            if (total == null)
            {
                return NotFound();
            }

            var links = new List<LinkDto>
            {
                LinkService.CreateLink(this, nameof(GetCartTotal), values: new { cartId }, rel: "self"),
                LinkService.CreateLink(this, nameof(GetCart), values: new { cartId }, rel: "cart"),
            };

            return Ok(CreateResource(total.Value, links));
        }
    }
}