using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces.Mappers.Product;
using ECommerceNetApp.Service.Queries.Product;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Product
{
    public class GetProductByIdQueryHandler(
        IProductCatalogUnitOfWork productCatalogUnitOfWork,
        IProductMapper productMapper) : IRequestHandler<GetProductByIdQuery, ProductDto?>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork;
        private readonly IProductMapper _productMapper = productMapper;

        public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var product = await _productCatalogUnitOfWork.ProductRepository
                .GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

            if (product == null)
            {
                return null;
            }

            return _productMapper.MapToProductDto(product);
        }
    }
}
