namespace ECommerceNetApp.Persistence.Interfaces.ProductCatalog
{
    public interface ICategoryDataSeeder
    {
        Task SeedCategoriesAsync(CancellationToken cancellationToken = default);
    }
}
