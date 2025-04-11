using ECommerceNetApp.Domain;

namespace ECommerceNetApp.Persistence
{
    public interface ICartRepository
    {
        Task<Cart?> GetCartAsync(string cartId);

        Task SaveCartAsync(Cart cart);

        Task DeleteCartAsync(string cartId);
    }
}
