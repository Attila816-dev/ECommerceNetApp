using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Service.DTO;

namespace ECommerceNetApp.Service.Queries.Category
{
    public record GetCategoriesByParentCategoryIdQuery(int? ParentCategoryId)
        : IQuery<IEnumerable<CategoryDto>>;
}
