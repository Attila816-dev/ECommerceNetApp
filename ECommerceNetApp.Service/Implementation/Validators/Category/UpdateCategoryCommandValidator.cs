using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Category;
using FluentValidation;

namespace ECommerceNetApp.Service.Validators.Category
{
    public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        private readonly ICategoryRepository _categoryRepository;

        public UpdateCategoryCommandValidator(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(CategoryEntity.MaxCategoryNameLength)
                .WithMessage($"Category Name cannot exceed {CategoryEntity.MaxCategoryNameLength} characters.");

            RuleFor(x => x.Id)
                .MustAsync(ExistingCategoryIdOrNullAsync)
                .WithMessage("Category does not exist.");

            RuleFor(x => x.ParentCategoryId)
                .MustAsync(ExistingParentCategoryIdOrNullAsync)
                .WithMessage("Parent Category does not exist.")
                .When(c => c.ParentCategoryId.HasValue);
        }

        private async Task<bool> ExistingCategoryIdOrNullAsync(UpdateCategoryCommand command, int categoryId, CancellationToken cancellationToken)
        {
            return await _categoryRepository.ExistsAsync(categoryId, cancellationToken).ConfigureAwait(false);
        }

        private async Task<bool> ExistingParentCategoryIdOrNullAsync(UpdateCategoryCommand command, int? parentCategoryId, CancellationToken cancellationToken)
        {
            if (!parentCategoryId.HasValue)
            {
                return true;
            }

            return await _categoryRepository.ExistsAsync(parentCategoryId.Value, cancellationToken).ConfigureAwait(false);
        }
    }
}
