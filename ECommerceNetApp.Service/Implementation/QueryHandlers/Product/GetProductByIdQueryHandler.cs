using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces.Mappers.Product;
using ECommerceNetApp.Service.Queries.Product;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Product
{
    public class GetProductByIdQueryHandler(
        IProductRepository productRepository,
        IProductMapper productMapper) : IRequestHandler<GetProductByIdQuery, ProductDto?>
    {
        private readonly IProductRepository _productRepository = productRepository;
        private readonly IProductMapper _productMapper = productMapper;

        public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (product == null)
            {
                return null;
            }

            return _productMapper.MapToProductDto(product);
        }
    }
}
