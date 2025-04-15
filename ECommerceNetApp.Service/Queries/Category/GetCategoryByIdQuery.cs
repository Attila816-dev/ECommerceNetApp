using ECommerceNetApp.Service.DTO;
using MediatR;

namespace ECommerceNetApp.Service.Queries.Category
{
    public record GetCategoryByIdQuery(int Id) : IRequest<CategoryDetailDto>;
}
