using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.Category;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Category
{
    public class GetCategoryByIdQueryHandler(ProductCatalogDbContext dbContext)
        : IQueryHandler<GetCategoryByIdQuery, CategoryDetailDto?>
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task<CategoryDetailDto?> HandleAsync(GetCategoryByIdQuery query, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(query);
            return await _dbContext.Categories.Include(c => c.ParentCategory).Include(c => c.Products)
                .AsNoTracking()
                .Where(c => c.Id == query.Id)
                .Select(c => new CategoryDetailDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ImageUrl = c.Image != null ? c.Image.Url : null,
                    ParentCategoryId = c.ParentCategoryId,
                    ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.Name : null,
                    Subcategories = c.SubCategories.Select(sc => new CategoryDto
                    {
                        Id = sc.Id,
                        Name = sc.Name,
                        ImageUrl = sc.Image != null ? sc.Image.Url : null,
                        ParentCategoryId = sc.ParentCategoryId,
                        ParentCategoryName = c.Name,
                    }),
                    Products = c.Products.Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        ImageUrl = p.Image != null ? p.Image.Url : null,
                        CategoryId = p.CategoryId,
                        CategoryName = c.Name,
                        Price = p.Price.Amount,
                        Currency = p.Price.Currency,
                        Amount = p.Amount,
                    }),
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
