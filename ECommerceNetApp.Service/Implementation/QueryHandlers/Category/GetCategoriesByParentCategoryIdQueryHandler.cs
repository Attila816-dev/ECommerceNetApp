using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.Category;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Category
{
    public class GetCategoriesByParentCategoryIdQueryHandler(ProductCatalogDbContext dbContext)
        : IQueryHandler<GetCategoriesByParentCategoryIdQuery, IEnumerable<CategoryDto>>
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task<IEnumerable<CategoryDto>> HandleAsync(GetCategoriesByParentCategoryIdQuery query, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(query);
            return await _dbContext.Categories.Include(c => c.ParentCategory)
                .AsNoTracking()
                .Where(c => c.ParentCategoryId == query.ParentCategoryId)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ImageUrl = c.Image != null ? c.Image.Url : null,
                    ParentCategoryId = c.ParentCategoryId,
                    ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.Name : null,
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
