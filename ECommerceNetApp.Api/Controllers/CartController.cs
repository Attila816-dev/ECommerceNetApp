using ECommerceNetApp.Service;
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
            try
            {
                var items = await _cartService.GetCartItemsAsync(cartId);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{cartId}/items")]
        public async Task<ActionResult> AddItemToCart(string cartId, CartItemDto item)
        {
            try
            {
                await _cartService.AddItemToCartAsync(cartId, item);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{cartId}/items/{itemId}")]
        public async Task<ActionResult> RemoveItemFromCart(string cartId, int itemId)
        {
            try
            {
                await _cartService.RemoveItemFromCartAsync(cartId, itemId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{cartId}/items/{itemId}")]
        public async Task<ActionResult> UpdateItemQuantity(string cartId, int itemId, [FromBody] int quantity)
        {
            try
            {
                await _cartService.UpdateItemQuantityAsync(cartId, itemId, quantity);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{cartId}/total")]
        public async Task<ActionResult<decimal>> GetCartTotal(string cartId)
        {
            try
            {
                var total = await _cartService.GetCartTotalAsync(cartId);
                return Ok(total);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
