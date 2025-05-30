using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Exceptions.Category;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.Product;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Product
{
    public class CreateProductCommandHandler(ProductCatalogDbContext dbContext)
        : ICommandHandler<CreateProductCommand, int>
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext;

        public async Task<int> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);

            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == command.CategoryId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                throw InvalidCategoryException.NotFound(command.CategoryId);
            }

            var product = ProductEntity.Create(
                command.Name,
                command.Description,
                command.ImageUrl != null ? ImageInfo.Create(command.ImageUrl) : null,
                category!,
                Money.Create(command.Price, command.Currency),
                command.Amount);

            await _dbContext.Products.AddAsync(product, cancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return product.Id;
        }
    }
}
