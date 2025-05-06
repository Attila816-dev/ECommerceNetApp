using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.Category;
using ECommerceNetApp.Service.Interfaces.Mappers.Category;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Category
{
    public class CreateCategoryCommandHandler(
            IProductCatalogUnitOfWork productCatalogUnitOfWork,
            ICategoryMapper categoryMapper)
        : ICommandHandler<CreateCategoryCommand, int>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork;
        private readonly ICategoryMapper _categoryMapper = categoryMapper;

        public async Task<int> HandleAsync(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            CategoryEntity? parentCategory = null;

            if (request.ParentCategoryId.HasValue)
            {
                parentCategory = await _productCatalogUnitOfWork.CategoryRepository.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken: cancellationToken).ConfigureAwait(false);
                if (parentCategory == null)
                {
                    throw new InvalidOperationException($"Parent category with id {request.ParentCategoryId.Value} not found");
                }
            }

            var category = _categoryMapper.MapToEntity(request, parentCategory);
            await _productCatalogUnitOfWork.CategoryRepository.AddAsync(category, cancellationToken).ConfigureAwait(false);
            await _productCatalogUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

            return category.Id;
        }
    }
}
