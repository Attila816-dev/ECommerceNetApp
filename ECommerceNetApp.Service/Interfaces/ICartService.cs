namespace ECommerceNetApp.Service.Interfaces
{
    public interface ICartService
    {
        Task<List<CartItemDto>?> GetCartItemsAsync(string cartId);

        Task AddItemToCartAsync(string cartId, CartItemDto item);

        Task RemoveItemFromCartAsync(string cartId, int itemId);

        Task UpdateItemQuantityAsync(string cartId, int itemId, int quantity);

        Task<decimal> GetCartTotalAsync(string cartId);
    }
}
