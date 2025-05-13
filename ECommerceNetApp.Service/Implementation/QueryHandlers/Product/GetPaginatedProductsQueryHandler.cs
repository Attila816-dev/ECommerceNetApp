using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.Product;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Product
{
    public class GetPaginatedProductsQueryHandler(ProductCatalogDbContext dbContext)
        : IQueryHandler<GetPaginatedProductsQuery, PaginationResult<ProductDto>>
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task<PaginationResult<ProductDto>> HandleAsync(
            GetPaginatedProductsQuery query,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(query);

            var productsQuery = _dbContext.Products.Include(p => p.Category).AsNoTracking();

            if (query.CategoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == query.CategoryId.Value);
            }

            var totalCount = await productsQuery.CountAsync(cancellationToken).ConfigureAwait(false);

            var products = await productsQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price.Amount,
                    Currency = p.Price.Currency,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : null,
                    Amount = p.Amount,
                    Description = p.Description,
                    ImageUrl = p.Image != null ? p.Image.Url : null,
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return new PaginationResult<ProductDto>(products, totalCount, query.PageNumber, query.PageSize);
        }
    }
}
