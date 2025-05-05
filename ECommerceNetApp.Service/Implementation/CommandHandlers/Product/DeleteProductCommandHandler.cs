using ECommerceNetApp.Domain.Exceptions.Product;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.Product;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Product
{
    public class DeleteProductCommandHandler(IProductCatalogUnitOfWork productCatalogUnitOfWork) : IRequestHandler<DeleteProductCommand>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork;

        public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
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
