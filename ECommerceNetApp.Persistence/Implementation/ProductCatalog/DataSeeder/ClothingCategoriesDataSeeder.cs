using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class ClothingCategoriesDataSeeder(IProductCatalogUnitOfWork productCatalogUnitOfWork) : ICategoryDataSeeder
    {
        public async Task SeedCategoriesAsync(CancellationToken cancellationToken = default)
        {
            var parentCategory = await productCatalogUnitOfWork.CategoryRepository
                .FirstOrDefaultAsync(c => c.Name == ProductCatalogConstants.ClothingCategoryName, cancellationToken: cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException(ProductCatalogConstants.ClothingCategoryName + " category not found.");

            // Clothing subcategories
            var mensClothingCategory = CategoryEntity.Create(
                ProductCatalogConstants.MensClothingSubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}mens.jpg"),
                parentCategory);

            await productCatalogUnitOfWork.CategoryRepository.AddAsync(mensClothingCategory, cancellationToken).ConfigureAwait(false);

            var womensClothingCategory = CategoryEntity.Create(
                ProductCatalogConstants.WomensClothingSubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}womens.jpg"),
                parentCategory);

            await productCatalogUnitOfWork.CategoryRepository.AddAsync(womensClothingCategory, cancellationToken).ConfigureAwait(false);

            var kidsClothingCategory = CategoryEntity.Create(
                ProductCatalogConstants.KidsClothingSubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}kids.jpg"),
                parentCategory);

            await productCatalogUnitOfWork.CategoryRepository.AddAsync(kidsClothingCategory, cancellationToken).ConfigureAwait(false);
            await productCatalogUnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
