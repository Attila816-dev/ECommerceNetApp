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

        public async Task HandleAsync(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var exists = await _productCatalogUnitOfWork.ProductRepository.ExistsAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (!exists)
            {
                throw InvalidProductException.NotFound(request.Id);
            }

            await _productCatalogUnitOfWork.ProductRepository.DeleteAsync(request.Id, cancellationToken).ConfigureAwait(false);
            await _productCatalogUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
