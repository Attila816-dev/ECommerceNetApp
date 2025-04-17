using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Category;
using FluentValidation;

namespace ECommerceNetApp.Service.Validators.Category
{
    public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
    {
        private readonly ICategoryRepository _categoryRepository;

        public CreateCategoryCommandValidator(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(CategoryEntity.MaxCategoryNameLength)
                .WithMessage($"Category name cannot exceed {CategoryEntity.MaxCategoryNameLength} characters.");

            RuleFor(x => x.ParentCategoryId)
                .MustAsync(ExistingCategoryIdOrNullAsync)
                .WithMessage("Parent Category does not exist.")
                .When(c => c.ParentCategoryId.HasValue);
        }

        private async Task<bool> ExistingCategoryIdOrNullAsync(CreateCategoryCommand command, int? parentCategoryId, CancellationToken cancellationToken)
        {
            if (!parentCategoryId.HasValue)
            {
                return true;
            }

            return await _categoryRepository.ExistsAsync(parentCategoryId.Value, cancellationToken).ConfigureAwait(false);
        }
    }
}
