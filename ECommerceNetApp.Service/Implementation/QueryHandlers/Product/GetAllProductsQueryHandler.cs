﻿using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.Product;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Product
{
    public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductDto>>
    {
        private readonly IProductRepository _productRepository;

        public GetAllProductsQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            IEnumerable<Domain.Entities.ProductEntity> products = await _productRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
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
