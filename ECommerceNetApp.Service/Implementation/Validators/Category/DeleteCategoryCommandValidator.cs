using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Category;
using FluentValidation;

namespace ECommerceNetApp.Service.Validators.Category
{
    public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;

        public DeleteCategoryCommandValidator(ICategoryRepository categoryRepository, IProductRepository productRepository)
        {
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));

            RuleFor(x => x.Id)
                .MustAsync(ExistingCategoryIdAsync)
                .WithMessage("Category does not exist.")
                .MustAsync(CategoryHasNoAssociatedProductsAsync)
                .WithMessage("Cannot delete category with associated products.")
                .MustAsync(CategoryHasNoChildCategories)
                .WithMessage("Cannot delete category with subcategories.");
        }

        private async Task<bool> ExistingCategoryIdAsync(DeleteCategoryCommand command, int categoryId, CancellationToken cancellationToken)
        {
            return await _categoryRepository.ExistsAsync(categoryId, cancellationToken).ConfigureAwait(false);
        }

        private async Task<bool> CategoryHasNoAssociatedProductsAsync(DeleteCategoryCommand command, int categoryId, CancellationToken cancellationToken)
        {
            return !(await _productRepository.ExistsAnyProductWithCategoryIdAsync(categoryId, cancellationToken).ConfigureAwait(false));
        }

        private async Task<bool> CategoryHasNoChildCategories(DeleteCategoryCommand command, int categoryId, CancellationToken cancellationToken)
        {
            // Check if category has subcategories
            var subcategories = await _categoryRepository.GetByParentCategoryIdAsync(command.Id, cancellationToken).ConfigureAwait(false);
            return subcategories == null || !subcategories.Any();
        }
    }
}
