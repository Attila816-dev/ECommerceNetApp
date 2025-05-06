using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.Category;
using FluentValidation;

namespace ECommerceNetApp.Service.Validators.Category
{
    public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork;

        public DeleteCategoryCommandValidator(IProductCatalogUnitOfWork productCatalogUnitOfWork)
        {
            _productCatalogUnitOfWork = productCatalogUnitOfWork ?? throw new ArgumentNullException(nameof(productCatalogUnitOfWork));

            RuleFor(x => x.Id)
                .MustAsync(ExistingCategoryIdAsync)
                .WithMessage("Category does not exist.")
                .MustAsync(CategoryHasNoChildCategories)
                .WithMessage("Cannot delete category with subcategories.");
        }

        private async Task<bool> ExistingCategoryIdAsync(DeleteCategoryCommand command, int categoryId, CancellationToken cancellationToken)
        {
            return await _productCatalogUnitOfWork.CategoryRepository.ExistsAsync(categoryId, cancellationToken).ConfigureAwait(false);
        }

        private async Task<bool> CategoryHasNoChildCategories(DeleteCategoryCommand command, int categoryId, CancellationToken cancellationToken)
        {
            // Check if category has subcategories
            var subcategories = await _productCatalogUnitOfWork.CategoryRepository.GetByParentCategoryIdAsync(command.Id, cancellationToken).ConfigureAwait(false);
            return subcategories == null || !subcategories.Any();
        }
    }
}
