using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.Category;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Category
{
    public class UpdateCategoryCommandHandler(
            IProductCatalogUnitOfWork productCatalogUnitOfWork)
        : IRequestHandler<UpdateCategoryCommand>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork;

        public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var existingCategory = await _productCatalogUnitOfWork.CategoryRepository.GetByIdAsync(request.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (existingCategory == null)
            {
                throw new InvalidOperationException($"Category with Id {request.Id} not found");
            }

            existingCategory.UpdateName(request.Name);
            existingCategory.UpdateImage(request.ImageUrl != null ? new ImageInfo(request.ImageUrl, null) : null);
            await UpdateParentCategoryAsync(request, existingCategory, cancellationToken).ConfigureAwait(false);
            _productCatalogUnitOfWork.CategoryRepository.Update(existingCategory);
            await _productCatalogUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task UpdateParentCategoryAsync(UpdateCategoryCommand request, CategoryEntity category, CancellationToken cancellationToken)
        {
            if (request.ParentCategoryId != category.ParentCategoryId)
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

                category.UpdateParentCategory(parentCategory);
            }
        }
    }
}
