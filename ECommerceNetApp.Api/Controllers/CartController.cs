using ECommerceNetApp.Service.Commands;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces;
using ECommerceNetApp.Service.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceNetApp.Api.Controllers
{
    [ApiController]
    [Route("api/carts")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
        }

        [HttpGet("{cartId}/items")]
        public async Task<ActionResult<List<CartItemDto>>> GetCartItems(string cartId)
        {
            var cartItems = await _cartService.GetCartItemsAsync(new GetCartItemsQuery(cartId)).ConfigureAwait(false);
            if (cartItems == null)
            {
                return NotFound();
            }

            return Ok(cartItems);
        }

        [HttpPost("{cartId}/items")]
        public async Task<ActionResult> AddItemToCart(string cartId, CartItemDto item)
        {
            await _cartService.AddItemToCartAsync(new AddCartItemCommand(cartId, item)).ConfigureAwait(false);
            return Ok();
        }

        [HttpDelete("{cartId}/items/{itemId}")]
        public async Task<ActionResult> RemoveItemFromCart(string cartId, int itemId)
        {
            await _cartService.RemoveItemFromCartAsync(new RemoveCartItemCommand(cartId, itemId)).ConfigureAwait(false);
            return Ok();
        }

        [HttpPut("{cartId}/items/{itemId}")]
        public async Task<ActionResult> UpdateItemQuantity(string cartId, int itemId, [FromBody] int quantity)
        {
            await _cartService.UpdateItemQuantityAsync(new UpdateCartItemQuantityCommand(cartId, itemId, quantity)).ConfigureAwait(false);
            return Ok();
        }

        [HttpGet("{cartId}/total")]
        public async Task<ActionResult<decimal>> GetCartTotal(string cartId)
        {
            var total = await _cartService.GetCartTotalAsync(new GetCartTotalQuery(cartId)).ConfigureAwait(false);
            return Ok(total);
        }
    }
}
