using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces.Mappers.Category;
using ECommerceNetApp.Service.Queries.Category;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Category
{
    public class GetCategoryByIdQueryHandler(
        IProductCatalogUnitOfWork productCatalogUnitOfWork,
        ICategoryMapper categoryMapper)
        : IQueryHandler<GetCategoryByIdQuery, CategoryDetailDto?>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork;
        private readonly ICategoryMapper _categoryMapper = categoryMapper;

        public async Task<CategoryDetailDto?> HandleAsync(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var category = await _productCatalogUnitOfWork.CategoryRepository.GetByIdAsync(request.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                return null;
            }

            var subcategories = await _productCatalogUnitOfWork.CategoryRepository.GetByParentCategoryIdAsync(category.Id, cancellationToken).ConfigureAwait(false);
            var products = await _productCatalogUnitOfWork.ProductRepository.GetProductsByCategoryIdAsync(category.Id, cancellationToken).ConfigureAwait(false);
            return _categoryMapper.MapToCategoryDetailDto(category, subcategories, products);
        }
    }
}
