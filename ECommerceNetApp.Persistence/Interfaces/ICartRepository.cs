using ECommerceNetApp.Domain.Entities;

namespace ECommerceNetApp.Persistence.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> GetByIdAsync(string cartId);

        Task SaveAsync(Cart cart);

        Task DeleteAsync(string cartId);
    }
}
