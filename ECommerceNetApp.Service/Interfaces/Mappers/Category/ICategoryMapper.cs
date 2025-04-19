using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Service.Commands.Category;
using ECommerceNetApp.Service.DTO;

namespace ECommerceNetApp.Service.Interfaces.Mappers.Category
{
    public interface ICategoryMapper
    {
        CategoryDto MapToDto(CategoryEntity category);

        CategoryEntity MapToEntity(CreateCategoryCommand command, CategoryEntity? parentCategory);

        CategoryDetailDto MapToCategoryDetailDto(CategoryEntity category, IEnumerable<CategoryEntity> subcategories, IEnumerable<ProductEntity> products);
    }
}
