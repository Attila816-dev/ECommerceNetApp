using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.Category;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Category
{
    public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDetailDto?>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;

        public GetCategoryByIdQueryHandler(
            ICategoryRepository categoryRepository,
            IProductRepository productRepository)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
        }

        public async Task<CategoryDetailDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                return null;
            }

            var subcategories = await _categoryRepository.GetByParentCategoryIdAsync(category.Id, cancellationToken).ConfigureAwait(false);
            var products = await _productRepository.GetProductsByCategoryIdAsync(category.Id, cancellationToken).ConfigureAwait(false);
            return MapCategoryDetailDto(category, subcategories, products);
        }

        private static CategoryDetailDto MapCategoryDetailDto(CategoryEntity category, IEnumerable<CategoryEntity> subcategories, IEnumerable<ProductEntity> products)
        {
            return new CategoryDetailDto
            {
                Id = category.Id,
                Name = category.Name,
                ImageUrl = category.ImageUrl,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.Name,
                Subcategories = subcategories?.Select(c => MapSubCategory(c, category)),
                Products = products?.Select(p => MapProduct(p, category)),
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

        private static ProductDto MapProduct(ProductEntity product, CategoryEntity category)
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
