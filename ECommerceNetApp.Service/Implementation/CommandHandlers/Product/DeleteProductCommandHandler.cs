using ECommerceNetApp.Domain.Exceptions.Product;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.Product;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Product
{
    public class DeleteProductCommandHandler(IProductCatalogUnitOfWork productCatalogUnitOfWork)
        : ICommandHandler<DeleteProductCommand>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork;

        public async Task HandleAsync(DeleteProductCommand command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);
            var exists = await _productCatalogUnitOfWork.ProductRepository.ExistsAsync(command.Id, cancellationToken).ConfigureAwait(false);
            if (!exists)
            {
                throw InvalidProductException.NotFound(command.Id);
            }

            await _productCatalogUnitOfWork.ProductRepository.DeleteAsync(command.Id, cancellationToken).ConfigureAwait(false);
            await _productCatalogUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
