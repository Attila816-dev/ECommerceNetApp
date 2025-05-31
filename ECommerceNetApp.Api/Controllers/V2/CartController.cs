using Asp.Versioning;
using ECommerceNetApp.Api.Authorization;
using ECommerceNetApp.Api.Model;
using ECommerceNetApp.Api.Services;
using ECommerceNetApp.Domain.Authorization;
using ECommerceNetApp.Service.Commands.Cart;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces;
using ECommerceNetApp.Service.Queries.Cart;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceNetApp.Api.Controllers.V2
{
    /// <summary>
    /// Controller for managing shopping carts in the E-commerce application (API Version 1).
    /// </summary>
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/carts")]
    public class CartController(IDispatcher dispatcher, IHateoasLinkService linkService)
        : BaseApiController(linkService, dispatcher)
    {
        /// <summary>
        /// Retrieves all items in a specific cart.
        /// </summary>
        /// <param name="cartId">The ID of the cart.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>A list of cart items with HATEOAS links.</returns>
        [HttpGet("{cartId}")]
        [RequirePermission(Permissions.Read, Resources.Cart)]
        [MapToApiVersion("2.0")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CollectionLinkedResourceDto<CartItemDto>>> GetCartItems(
            string cartId,
            CancellationToken cancellationToken)
        {
            var cartItems = await Dispatcher.SendQueryAsync<GetCartItemsQuery, List<CartItemDto>?>(new GetCartItemsQuery(cartId), cancellationToken).ConfigureAwait(false);
            if (cartItems == null)
            {
                return NotFound();
            }

            var links = new List<LinkDto>
            {
                LinkService.CreateLink(this, nameof(GetCartItems), values: new { cartId }, rel: "self"),
                LinkService.CreateLink(this, nameof(AddItemToCart), values: new { cartId }, rel: "add_item", method: "POST"),
                LinkService.CreateLink(this, nameof(GetCartTotal), values: new { cartId }, rel: "cart_total"),
            };

            return Ok(CreateCollectionResource(cartItems, links));
        }

        /// <summary>
        /// Adds an item to a cart.
        /// </summary>
        /// <param name="cartId">The ID of the cart.</param>
        /// <param name="cartItem">The cart item to add.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>The created resource with links.</returns>
        [HttpPost("{cartId}/items")]
        [RequirePermission(Permissions.Update, Resources.Cart)]
        [MapToApiVersion("2.0")]
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
                LinkService.CreateLink(this, nameof(GetCartItems), values: new { cartId }, rel: "items"),
                LinkService.CreateLink(this, nameof(GetCartTotal), values: new { cartId }, rel: "total"),
            };

            var resource = CreateResource(cartId, links);

            return CreatedAtAction(nameof(GetCartItems), new { cartId, version = "2.0" }, resource);
        }

        /// <summary>
        /// Removes an item from a cart.
        /// </summary>
        /// <param name="cartId">The ID of the cart.</param>
        /// <param name="itemId">The ID of the item to remove.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>No content if the deletion is successful.</returns>
        [HttpDelete("{cartId}/items/{itemId}")]
        [RequirePermission(Permissions.Update, Resources.Cart)]
        [MapToApiVersion("2.0")]
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
        /// Updates the quantity of a cart item.
        /// </summary>
        /// <param name="cartId">The ID of the cart.</param>
        /// <param name="itemId">The ID of the item to update.</param>
        /// <param name="quantity">The new quantity.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>No content if the update is successful.</returns>
        [HttpPut("{cartId}/items/{itemId}")]
        [RequirePermission(Permissions.Update, Resources.Cart)]
        [MapToApiVersion("2.0")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateItemQuantity(
            string cartId,
            int itemId,
            [FromBody] int quantity,
            CancellationToken cancellationToken)
        {
            await Dispatcher.SendCommandAsync(new UpdateCartItemQuantityCommand(cartId, itemId, quantity), cancellationToken).ConfigureAwait(false);
            return NoContent();
        }

        /// <summary>
        /// Gets the total value of a cart.
        /// </summary>
        /// <param name="cartId">The ID of the cart.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>The cart total with links.</returns>
        [HttpGet("{cartId}/total")]
        [RequirePermission(Permissions.Read, Resources.Cart)]
        [MapToApiVersion("2.0")]
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
                LinkService.CreateLink(this, nameof(GetCartItems), values: new { cartId }, rel: "items"),
            };

            return Ok(CreateResource(total.Value, links));
        }

        /// <summary>
        /// Gets a specific item from a cart.
        /// </summary>
        /// <param name="cartId">The ID of the cart.</param>
        /// <param name="itemId">The ID of the item.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>The cart item with links.</returns>
        [HttpGet("{cartId}/items/{itemId}")]
        [RequirePermission(Permissions.Read, Resources.Cart)]
        [MapToApiVersion("2.0")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LinkedResourceDto<CartItemDto>>> GetCartItem(
            string cartId,
            int itemId,
            CancellationToken cancellationToken)
        {
            var query = new GetCartItemQuery(cartId, itemId);
            var cartItem = await Dispatcher.SendQueryAsync<GetCartItemQuery, CartItemDto?>(query, cancellationToken).ConfigureAwait(false);

            if (cartItem == null)
            {
                return NotFound();
            }

            var links = new List<LinkDto>
            {
                LinkService.CreateLink(this, nameof(GetCartItem), values: new { cartId, itemId }, rel: "self"),
                LinkService.CreateLink(this, nameof(UpdateItemQuantity), values: new { cartId, itemId }, rel: "update", method: "PUT"),
                LinkService.CreateLink(this, nameof(RemoveItemFromCart), values: new { cartId, itemId }, rel: "delete", method: "DELETE"),
                LinkService.CreateLink(this, nameof(GetCartItems), values: new { cartId }, rel: "cart_items"),
            };

            return Ok(CreateResource(cartItem, links));
        }
    }
}