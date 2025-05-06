using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.Product;
using ECommerceNetApp.Service.Interfaces.Mappers.Product;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Product
{
    public class CreateProductCommandHandler(
            IProductCatalogUnitOfWork productCatalogUnitOfWork,
            IProductMapper productMapper)
        : ICommandHandler<CreateProductCommand, int>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork;
        private readonly IProductMapper _productMapper = productMapper;

        public async Task<int> HandleAsync(CreateProductCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var category = await _productCatalogUnitOfWork.CategoryRepository.GetByIdAsync(request.CategoryId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                throw new InvalidOperationException($"Category with id {request.CategoryId} not found");
            }

            var productEntity = _productMapper.MapToEntity(request, category);
            await _productCatalogUnitOfWork.ProductRepository.AddAsync(productEntity, cancellationToken).ConfigureAwait(false);
            await _productCatalogUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
            return productEntity.Id;
        }
    }
}
