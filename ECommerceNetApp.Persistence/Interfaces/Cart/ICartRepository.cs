using ECommerceNetApp.Domain.Entities;

namespace ECommerceNetApp.Persistence.Interfaces.Cart
{
    public interface ICartRepository
    {
        Task<CartEntity?> GetByIdAsync(string cartId, CancellationToken cancellationToken);

        Task SaveAsync(CartEntity cart, CancellationToken cancellationToken);

        Task DeleteAsync(string cartId, CancellationToken cancellationToken);

        Task<bool> ExistsAsync(string cartId, CancellationToken cancellationToken);
    }
}
