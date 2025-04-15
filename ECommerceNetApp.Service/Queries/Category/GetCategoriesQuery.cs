using ECommerceNetApp.Service.DTO;
using MediatR;

namespace ECommerceNetApp.Service.Queries.Category
{
    public record GetCategoriesQuery(int? ParentCategoryId)
        : IRequest<IEnumerable<CategoryDto>>;
}
