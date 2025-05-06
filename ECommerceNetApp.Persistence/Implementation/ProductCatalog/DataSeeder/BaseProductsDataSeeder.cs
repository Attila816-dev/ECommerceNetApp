using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal abstract class BaseProductsDataSeeder(IProductCatalogUnitOfWork productCatalogUnitOfWork)
    {
        public IProductCatalogUnitOfWork ProductCatalogUnitOfWork => productCatalogUnitOfWork;

        protected virtual async Task<CategoryEntity> GetCategoryAsync(string categoryName, CancellationToken cancellationToken)
        {
            var category = await ProductCatalogUnitOfWork.CategoryRepository
                .FirstOrDefaultAsync(c => c.Name == categoryName, cancellationToken: cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException(categoryName + " category not found.");
            return category;
        }

        protected virtual async Task AddProductAsync(ProductEntity product, CancellationToken cancellationToken)
        {
            await ProductCatalogUnitOfWork.ProductRepository.AddAsync(product, cancellationToken).ConfigureAwait(false);
        }

        protected virtual async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await ProductCatalogUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
