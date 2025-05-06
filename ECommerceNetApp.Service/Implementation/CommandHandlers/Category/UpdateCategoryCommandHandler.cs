using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.Category;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Category
{
    public class UpdateCategoryCommandHandler(
            IProductCatalogUnitOfWork productCatalogUnitOfWork)
        : ICommandHandler<UpdateCategoryCommand>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork;

        public async Task HandleAsync(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var existingCategory = await _productCatalogUnitOfWork.CategoryRepository.GetByIdAsync(request.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (existingCategory == null)
            {
                throw new InvalidOperationException($"Category with Id {request.Id} not found");
            }

            var imageInfo = request.ImageUrl != null ? ImageInfo.Create(request.ImageUrl) : null;
            var parentCategory = await GetParentCategoryAsync(request, existingCategory, cancellationToken).ConfigureAwait(false);
            existingCategory.Update(request.Name, imageInfo, parentCategory);
            _productCatalogUnitOfWork.CategoryRepository.Update(existingCategory);
            await _productCatalogUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task<CategoryEntity?> GetParentCategoryAsync(UpdateCategoryCommand request, CategoryEntity category, CancellationToken cancellationToken)
        {
            CategoryEntity? parentCategory = null;
            if (request.ParentCategoryId.HasValue)
            {
                parentCategory = await _productCatalogUnitOfWork.CategoryRepository
                    .GetByIdAsync(request.ParentCategoryId.Value, cancellationToken: cancellationToken).ConfigureAwait(false);
                if (parentCategory == null)
                {
                    throw new InvalidOperationException($"Parent category with id {request.ParentCategoryId.Value} not found");
                }
            }

            return parentCategory;
        }
    }
}
