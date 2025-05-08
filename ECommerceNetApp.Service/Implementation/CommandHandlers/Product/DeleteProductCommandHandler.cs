using ECommerceNetApp.Domain.Exceptions.Product;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.Product;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Product
{
    public class DeleteProductCommandHandler(ProductCatalogDbContext dbContext)
        : ICommandHandler<DeleteProductCommand>
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext;

        public async Task HandleAsync(DeleteProductCommand command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);
            var existingProduct = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken).ConfigureAwait(false);
            if (existingProduct == null)
            {
                throw InvalidProductException.NotFound(command.Id);
            }

            existingProduct.MarkAsDeleted();
            _dbContext.Products.Remove(existingProduct);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
