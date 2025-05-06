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

        public async Task HandleAsync(UpdateCategoryCommand command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);

            var existingCategory = await _productCatalogUnitOfWork.CategoryRepository.GetByIdAsync(command.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (existingCategory == null)
            {
                throw new InvalidOperationException($"Category with Id {command.Id} not found");
            }

            var imageInfo = command.ImageUrl != null ? ImageInfo.Create(command.ImageUrl) : null;
            var parentCategory = await GetParentCategoryAsync(command, existingCategory, cancellationToken).ConfigureAwait(false);
            existingCategory.Update(command.Name, imageInfo, parentCategory);
            _productCatalogUnitOfWork.CategoryRepository.Update(existingCategory);
            await _productCatalogUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task<CategoryEntity?> GetParentCategoryAsync(UpdateCategoryCommand command, CategoryEntity category, CancellationToken cancellationToken)
        {
            CategoryEntity? parentCategory = null;
            if (command.ParentCategoryId.HasValue)
            {
                parentCategory = await _productCatalogUnitOfWork.CategoryRepository
                    .GetByIdAsync(command.ParentCategoryId.Value, cancellationToken: cancellationToken).ConfigureAwait(false);
                if (parentCategory == null)
                {
                    throw new InvalidOperationException($"Parent category with id {command.ParentCategoryId.Value} not found");
                }
            }

            return parentCategory;
        }
    }
}
