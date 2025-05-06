using ECommerceNetApp.Domain.Interfaces;

namespace ECommerceNetApp.Service.Queries.Cart
{
    public record GetCartTotalQuery(string CartId) : IQuery<decimal?>;
}
