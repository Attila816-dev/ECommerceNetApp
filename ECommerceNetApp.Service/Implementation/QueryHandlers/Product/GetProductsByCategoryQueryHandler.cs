using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.Product;
using MediatR;
using ProductEntity = ECommerceNetApp.Domain.Entities.Product;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Product
{
    public class GetProductsByCategoryQueryHandler : IRequestHandler<GetProductsByCategoryQuery, IEnumerable<ProductDto>>
    {
        private readonly IProductRepository _productRepository;

        public GetProductsByCategoryQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<ProductDto>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            IEnumerable<Domain.Entities.Product> products = await _productRepository.GetProductsByCategoryIdAsync(request.CategoryId, cancellationToken).ConfigureAwait(false);
            return products.Select(p => MapProductDto(p)).ToList();
        }

        private static ProductDto MapProductDto(ProductEntity product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                Price = product.Price,
                Amount = product.Amount,
            };
        }
    }
}
