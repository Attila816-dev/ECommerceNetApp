using MediatR;

namespace ECommerceNetApp.Service.Queries
{
    public record GetCartTotalQuery(string CartId) : IRequest<decimal>;
}
