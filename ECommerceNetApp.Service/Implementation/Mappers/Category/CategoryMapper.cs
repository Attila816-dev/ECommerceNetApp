using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Service.Commands.Category;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces.Mappers.Category;

namespace ECommerceNetApp.Service.Implementation.Mappers.Category
{
    public class CategoryMapper : ICategoryMapper
    {
        public CategoryDto MapToDto(CategoryEntity category)
        {
            ArgumentNullException.ThrowIfNull(category);

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                ImageUrl = category.ImageUrl,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.Name,
            };
        }

        public CategoryEntity MapToEntity(CreateCategoryCommand command, CategoryEntity? parentCategory)
        {
            ArgumentNullException.ThrowIfNull(command);
            return new CategoryEntity(command.Name, command.ImageUrl, parentCategory);
        }

        public CategoryDetailDto MapToCategoryDetailDto(CategoryEntity category, IEnumerable<CategoryEntity> subcategories, IEnumerable<ProductEntity> products)
        {
            ArgumentNullException.ThrowIfNull(category);
            ArgumentNullException.ThrowIfNull(subcategories);
            ArgumentNullException.ThrowIfNull(products);

            return new CategoryDetailDto
            {
                Id = category.Id,
                Name = category.Name,
                ImageUrl = category.ImageUrl,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.Name,
                Subcategories = subcategories?.Select(c => MapSubCategory(c, category)),
                Products = products?.Select(p => MapCategoryProductToProductDto(p, category)),
            };
        }

        private static CategoryDto MapSubCategory(CategoryEntity category, CategoryEntity parentCategory)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                ImageUrl = category.ImageUrl,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = parentCategory.Name,
            };
        }

        private static ProductDto MapCategoryProductToProductDto(ProductEntity product, CategoryEntity category)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                CategoryName = category.Name,
                Price = product.Price,
                Amount = product.Amount,
            };
        }
    }
}
