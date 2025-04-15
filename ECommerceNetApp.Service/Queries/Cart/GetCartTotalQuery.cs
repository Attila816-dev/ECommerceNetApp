using MediatR;

namespace ECommerceNetApp.Service.Queries.Cart
{
    public record GetCartTotalQuery(string CartId) : IRequest<decimal?>;
}
