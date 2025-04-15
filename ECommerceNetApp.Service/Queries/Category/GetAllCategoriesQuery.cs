using ECommerceNetApp.Service.DTO;
using MediatR;

namespace ECommerceNetApp.Service.Queries.Category
{
    public record GetAllCategoriesQuery() : IRequest<IEnumerable<CategoryDto>>;
}
