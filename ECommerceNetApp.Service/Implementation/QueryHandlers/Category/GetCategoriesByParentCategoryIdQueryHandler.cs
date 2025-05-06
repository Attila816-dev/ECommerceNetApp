using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces.Mappers.Category;
using ECommerceNetApp.Service.Queries.Category;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Category
{
    public class GetCategoriesByParentCategoryIdQueryHandler(
        IProductCatalogUnitOfWork productCatalogUnitOfWork,
        ICategoryMapper categoryMapper)
        : IQueryHandler<GetCategoriesByParentCategoryIdQuery, IEnumerable<CategoryDto>>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork;
        private readonly ICategoryMapper _categoryMapper = categoryMapper;

        public async Task<IEnumerable<CategoryDto>> HandleAsync(GetCategoriesByParentCategoryIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            IEnumerable<CategoryEntity> categories = await _productCatalogUnitOfWork.CategoryRepository
                .GetByParentCategoryIdAsync(request.ParentCategoryId, cancellationToken).ConfigureAwait(false);
            return categories.Select(_categoryMapper.MapToDto).ToList();
        }
    }
}
