using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces.Mappers.Category;
using ECommerceNetApp.Service.Queries.Category;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Category
{
    public class GetAllCategoriesQueryHandler(
        IProductCatalogUnitOfWork productCatalogUnitOfWork,
        ICategoryMapper categoryMapper)
        : IRequestHandler<GetAllCategoriesQuery, IEnumerable<CategoryDto>>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork;
        private readonly ICategoryMapper _categoryMapper = categoryMapper;

        public async Task<IEnumerable<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            IEnumerable<CategoryEntity> categories = await _productCatalogUnitOfWork.CategoryRepository
                .GetAllAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            return categories.Select(_categoryMapper.MapToDto).ToList();
        }
    }
}
