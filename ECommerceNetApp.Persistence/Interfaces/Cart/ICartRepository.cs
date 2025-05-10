using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;

namespace ECommerceNetApp.Persistence.Interfaces.Cart
{
    public interface ICartRepository
    {
        Task<CartEntity?> GetByIdAsync(string cartId, CancellationToken cancellationToken);

        Task SaveAsync(CartEntity cart, CancellationToken cancellationToken);

        Task DeleteAsync(string cartId, CancellationToken cancellationToken);

        Task<bool> ExistsAsync(string cartId, CancellationToken cancellationToken);

        Task<int> CountAsync(CancellationToken cancellationToken);

        Task<IEnumerable<CartEntity>> GetCartsContainingProductAsync(int productId, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a specific item from the cart.
        /// </summary>
        /// <param name="cartId">The ID of the cart.</param>
        /// <param name="itemId">The ID of the item.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The cart item if found, otherwise null.</returns>
        Task<CartItem?> GetCartItemAsync(string cartId, int itemId, CancellationToken cancellationToken);
    }
}
