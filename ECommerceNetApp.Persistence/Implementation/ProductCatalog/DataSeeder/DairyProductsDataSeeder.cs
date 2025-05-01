using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class DairyProductsDataSeeder(ProductCatalogDbContext dbContext) : IProductDataSeeder
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == ProductCatalogConstants.DairySubCategoryName, cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                throw new InvalidOperationException(ProductCatalogConstants.DairySubCategoryName + " category not found.");
            }

            var milk = ProductEntity.Create(
                "Semi-Skimmed Milk 2L",
                "Fresh semi-skimmed milk, locally sourced.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}milk.jpg"),
                category,
                Money.From(1.85m),
                50);

            var eggs = ProductEntity.Create(
                "Free Range Eggs 12pk",
                "Large free-range eggs from happy hens.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}eggs.jpg"),
                category,
                Money.From(2.75m),
                40);

            var cheese = ProductEntity.Create(
                "Mature Cheddar 400g",
                "Tangy mature cheddar cheese, perfect for sandwiches or cooking.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}cheese.jpg"),
                category,
                Money.From(3.50m),
                60);

            await _dbContext.Products.AddRangeAsync(milk, eggs, cheese).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
