using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces.Mappers.Product;
using ECommerceNetApp.Service.Queries.Product;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Product
{
    public class GetPaginatedProductsQueryHandler(
        IProductCatalogUnitOfWork productCatalogUnitOfWork,
        IProductMapper productMapper) :
        IQueryHandler<GetPaginatedProductsQuery, PaginationResult<ProductDto>>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork;
        private readonly IProductMapper _productMapper = productMapper;

        public async Task<PaginationResult<ProductDto>> HandleAsync(
            GetPaginatedProductsQuery query,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(query);

            var (products, totalCount) = await _productCatalogUnitOfWork.ProductRepository
                .GetPaginatedProductsAsync(
                    query.PageNumber,
                    query.PageSize,
                    query.CategoryId,
                    cancellationToken)
                .ConfigureAwait(false);

            var productDtos = products.Select(_productMapper.MapToProductDto).ToList();

            return new PaginationResult<ProductDto>(
                productDtos,
                totalCount,
                query.PageNumber,
                query.PageSize);
        }
    }
}
