using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.Product;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Product
{
    public class GetProductsByCategoryQueryHandler(ProductCatalogDbContext dbContext)
        : IQueryHandler<GetProductsByCategoryQuery, IEnumerable<ProductDto>>
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task<IEnumerable<ProductDto>> HandleAsync(GetProductsByCategoryQuery query, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(query);
            var products = await _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.CategoryId == query.CategoryId)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    ImageUrl = p.Image != null ? p.Image.Url : null,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : null,
                    Price = p.Price.Amount,
                    Currency = p.Price.Currency,
                    Amount = p.Amount,
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            return products;
        }
    }
}
