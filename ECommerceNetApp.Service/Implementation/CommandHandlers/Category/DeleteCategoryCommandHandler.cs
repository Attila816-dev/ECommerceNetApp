using ECommerceNetApp.Domain.Exceptions.Category;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.Category;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Category
{
    public class DeleteCategoryCommandHandler(ProductCatalogDbContext dbContext)
        : ICommandHandler<DeleteCategoryCommand>
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext;

        public async Task HandleAsync(DeleteCategoryCommand command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);

            var category = (await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken).ConfigureAwait(false))
                ?? throw InvalidCategoryException.NotFound(command.Id);

            // Get all products related to this category
            var products = await _dbContext.Products
                .Where(p => p.CategoryId == command.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            // Delete all related products first
            foreach (var product in products)
            {
                product.MarkAsDeleted();
            }

            _dbContext.Products.RemoveRange(products);

            category.MarkAsDeleted();
            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
