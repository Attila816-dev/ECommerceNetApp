using ECommerceNetApp.Service.DTO;
using MediatR;

namespace ECommerceNetApp.Service.Queries.Category
{
    public record GetCategoriesByParentCategoryIdQuery(int? ParentCategoryId)
        : IRequest<IEnumerable<CategoryDto>>;
}
