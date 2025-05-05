namespace ECommerceNetApp.Service.DTO
{
    /// <summary>
    /// Data transfer object for a shopping cart.
    /// </summary>
    public class CartDto(string cartId, List<CartItemDto> items)
    {
        /// <summary>
        /// Gets the unique identifier of the cart.
        /// </summary>
        public string CartId { get; } = cartId;

        /// <summary>
        /// Gets the list of items in the cart.
        /// </summary>
        public List<CartItemDto> Items { get; } = items;
    }
}
