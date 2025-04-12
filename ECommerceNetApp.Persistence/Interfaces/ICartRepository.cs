using ECommerceNetApp.Domain.Entities;

namespace ECommerceNetApp.Persistence.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> GetCartAsync(string cartId);

        Task SaveCartAsync(Cart cart);

        Task DeleteCartAsync(string cartId);
    }
}
