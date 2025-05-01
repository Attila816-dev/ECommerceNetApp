namespace ECommerceNetApp.Persistence.Interfaces.ProductCatalog
{
    public interface IProductDataSeeder
    {
        Task SeedProductsAsync(CancellationToken cancellationToken = default);
    }
}
