using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.Product;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Product
{
    public class GetProductByIdQueryHandler(ProductCatalogDbContext dbContext)
        : IQueryHandler<GetProductByIdQuery, ProductDto?>
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task<ProductDto?> HandleAsync(GetProductByIdQuery query, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(query);

            return await _dbContext.Products
                .AsNoTracking()
                .Where(p => p.Id == query.Id)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price.Amount,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : null,
                    Amount = p.Amount,
                    Description = p.Description,
                    ImageUrl = p.Image != null ? p.Image.Url : null,
                    Currency = p.Price.Currency,
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
