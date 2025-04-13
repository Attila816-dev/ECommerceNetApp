using ECommerceNetApp.Service.Commands;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries;

namespace ECommerceNetApp.Service.Interfaces
{
    public interface ICartService
    {
        Task<List<CartItemDto>?> GetCartItemsAsync(GetCartItemsQuery query);

        Task<decimal> GetCartTotalAsync(GetCartTotalQuery query);

        Task AddItemToCartAsync(AddCartItemCommand command);

        Task RemoveItemFromCartAsync(RemoveCartItemCommand command);

        Task UpdateItemQuantityAsync(UpdateCartItemQuantityCommand command);
    }
}
