using ECommerceNetApp.Domain.Exceptions.Category;
using ECommerceNetApp.Domain.Exceptions.Product;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.Product;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Product
{
    public class UpdateProductCommandHandler(ProductCatalogDbContext dbContext)
        : ICommandHandler<UpdateProductCommand>
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext;

        public async Task HandleAsync(UpdateProductCommand command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);

            var product = (await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken: cancellationToken).ConfigureAwait(false))
                ?? throw InvalidProductException.NotFound(command.Id);

            var category = (await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == command.CategoryId, cancellationToken: cancellationToken).ConfigureAwait(false))
                ?? throw InvalidCategoryException.NotFound(command.CategoryId);

            var imageInfo = command.ImageUrl != null ? ImageInfo.Create(command.ImageUrl) : null;
            var money = Money.Create(command.Price, command.Currency);

            product.Update(command.Name, command.Description, imageInfo, category, money, command.Amount);
            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
