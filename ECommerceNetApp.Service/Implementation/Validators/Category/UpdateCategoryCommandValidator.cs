using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.Category;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Service.Validators.Category
{
    public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        private readonly ProductCatalogDbContext _dbContext;

        public UpdateCategoryCommandValidator(ProductCatalogDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

            RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(CategoryEntity.MaxCategoryNameLength)
                .WithMessage($"Category Name cannot exceed {CategoryEntity.MaxCategoryNameLength} characters.");

            RuleFor(x => x.Id)
                .MustAsync(ExistingCategoryIdAsync)
                .WithMessage("Category does not exist.");

            RuleFor(x => x.ParentCategoryId)
                .Must((command, parentCategoryId) => parentCategoryId == null || parentCategoryId != command.Id)
                .WithMessage("A category cannot be its own parent.")
                .MustAsync(ExistingParentCategoryIdOrNullAsync)
                .WithMessage("Parent Category does not exist.")
                .When(c => c.ParentCategoryId.HasValue);
        }

        private async Task<bool> ExistingCategoryIdAsync(UpdateCategoryCommand command, int categoryId, CancellationToken cancellationToken)
        {
            return await _dbContext.Categories.AnyAsync(c => c.Id == categoryId, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private async Task<bool> ExistingParentCategoryIdOrNullAsync(UpdateCategoryCommand command, int? parentCategoryId, CancellationToken cancellationToken)
        {
            if (!parentCategoryId.HasValue)
            {
                return true;
            }

            return await _dbContext.Categories.AnyAsync(c => c.Id == parentCategoryId.Value, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
