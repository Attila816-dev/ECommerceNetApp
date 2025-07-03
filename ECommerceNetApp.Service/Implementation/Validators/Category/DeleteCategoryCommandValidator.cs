using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.Category;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Service.Implementation.Validators.Category
{
    public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
    {
        private readonly ProductCatalogDbContext _dbContext;

        public DeleteCategoryCommandValidator(ProductCatalogDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

            RuleFor(x => x.Id)
                .MustAsync(ExistingCategoryIdAsync)
                .WithMessage("Category does not exist.")
                .MustAsync(CategoryHasNoChildCategories)
                .WithMessage("Cannot delete category with subcategories.");
        }

        private async Task<bool> ExistingCategoryIdAsync(DeleteCategoryCommand command, int categoryId, CancellationToken cancellationToken)
        {
            return await _dbContext.Categories.AnyAsync(c => c.Id == categoryId, cancellationToken).ConfigureAwait(false);
        }

        private async Task<bool> CategoryHasNoChildCategories(DeleteCategoryCommand command, int categoryId, CancellationToken cancellationToken)
        {
            // Check if category has subcategories
            var hasSubcategories = await _dbContext.Categories.AnyAsync(c => c.ParentCategoryId == command.Id, cancellationToken).ConfigureAwait(false);
            return !hasSubcategories;
        }
    }
}
