using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class MeatProductsDataSeeder(ProductCatalogDbContext dbContext) : IProductDataSeeder
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == ProductCatalogConstants.MeatSubCategoryName, cancellationToken).ConfigureAwait(false);

            if (category == null)
            {
                throw new InvalidOperationException(ProductCatalogConstants.MeatSubCategoryName + " category not found.");
            }

            var chicken = ProductEntity.Create(
                "Chicken Breast Fillets 500g",
                "Fresh chicken breast fillets, perfect for a variety of dishes.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}chicken.jpg"),
                category,
                Money.From(5.95m),
                20);

            var beef = ProductEntity.Create(
                "Lean Beef Mince 500g",
                "Quality lean beef mince, ideal for bolognese or burgers.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}beef.jpg"),
                category,
                Money.From(4.50m),
                15);

            await _dbContext.Products.AddRangeAsync(chicken, beef).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
