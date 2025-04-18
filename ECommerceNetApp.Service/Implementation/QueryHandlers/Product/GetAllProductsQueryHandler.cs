using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces.Mappers.Product;
using ECommerceNetApp.Service.Queries.Product;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Product
{
    public class GetAllProductsQueryHandler(
        IProductRepository productRepository,
        IProductMapper productMapper)
        : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductDto>>
    {
        private readonly IProductRepository _productRepository = productRepository;
        private readonly IProductMapper _productMapper = productMapper;

        public async Task<IEnumerable<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            IEnumerable<ProductEntity> products = await _productRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
            return products.Select(_productMapper.MapToProductDto).ToList();
        }
    }
}
