using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces.Mappers.Product;
using ECommerceNetApp.Service.Queries.Product;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Product
{
    public class GetProductsByCategoryQueryHandler(
        IProductCatalogUnitOfWork productCatalogUnitOfWork,
        IProductMapper productMapper)
        : IRequestHandler<GetProductsByCategoryQuery, IEnumerable<ProductDto>>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork;
        private readonly IProductMapper _productMapper = productMapper;

        public async Task<IEnumerable<ProductDto>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            IEnumerable<ProductEntity> products = await _productCatalogUnitOfWork.ProductRepository
                .GetProductsByCategoryIdAsync(request.CategoryId, cancellationToken).ConfigureAwait(false);
            return products.Select(_productMapper.MapToProductDto).ToList();
        }
    }
}
