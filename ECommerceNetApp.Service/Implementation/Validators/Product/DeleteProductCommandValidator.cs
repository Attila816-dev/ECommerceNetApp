using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.Product;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Service.Validators.Product
{
    public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
    {
        private readonly ProductCatalogDbContext _dbContext;

        public DeleteProductCommandValidator(ProductCatalogDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

            RuleFor(x => x.Id)
                .MustAsync(ExistingProductIdAsync)
                .WithMessage("Product does not exist.");
        }

        private async Task<bool> ExistingProductIdAsync(DeleteProductCommand command, int productId, CancellationToken cancellationToken)
        {
            return await _dbContext.Products.AnyAsync(c => c.Id == productId, cancellationToken).ConfigureAwait(false);
        }
    }
}
