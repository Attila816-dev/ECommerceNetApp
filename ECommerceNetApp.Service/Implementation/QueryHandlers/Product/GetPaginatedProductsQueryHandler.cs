using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces.Mappers.Product;
using ECommerceNetApp.Service.Queries.Product;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Product
{
    public class GetPaginatedProductsQueryHandler(
        IProductCatalogUnitOfWork productCatalogUnitOfWork,
        IProductMapper productMapper) :
        IRequestHandler<GetPaginatedProductsQuery, PaginationResult<ProductDto>>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork;
        private readonly IProductMapper _productMapper = productMapper;

        public async Task<PaginationResult<ProductDto>> Handle(
            GetPaginatedProductsQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var (products, totalCount) = await _productCatalogUnitOfWork.ProductRepository
                .GetPaginatedProductsAsync(
                    request.PageNumber,
                    request.PageSize,
                    request.CategoryId,
                    cancellationToken)
                .ConfigureAwait(false);

            var productDtos = products.Select(_productMapper.MapToProductDto).ToList();

            return new PaginationResult<ProductDto>(
                productDtos,
                totalCount,
                request.PageNumber,
                request.PageSize);
        }
    }
}
