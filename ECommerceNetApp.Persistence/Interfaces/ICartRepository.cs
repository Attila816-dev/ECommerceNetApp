using ECommerceNetApp.Domain.Entities;

namespace ECommerceNetApp.Persistence.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> GetByIdAsync(string cartId, CancellationToken cancellationToken);

        Task SaveAsync(Cart cart, CancellationToken cancellationToken);

        Task DeleteAsync(string cartId, CancellationToken cancellationToken);
    }
}
