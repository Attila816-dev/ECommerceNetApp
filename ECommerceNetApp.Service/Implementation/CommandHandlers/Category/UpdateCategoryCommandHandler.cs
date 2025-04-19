using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.Category;
using FluentValidation;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Category
{
    public class UpdateCategoryCommandHandler(
            IProductCatalogUnitOfWork productCatalogUnitOfWork,
            IValidator<UpdateCategoryCommand> validator)
        : IRequestHandler<UpdateCategoryCommand>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork;
        private readonly IValidator<UpdateCategoryCommand> _validator = validator;

        public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existingCategory = await _productCatalogUnitOfWork.CategoryRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (existingCategory == null)
            {
                throw new InvalidOperationException($"Category with Id {request.Id} not found");
            }

            existingCategory.UpdateName(request.Name);
            existingCategory.UpdateImage(request.ImageUrl);
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
                        .GetByIdAsync(request.ParentCategoryId.Value, cancellationToken).ConfigureAwait(false);
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
