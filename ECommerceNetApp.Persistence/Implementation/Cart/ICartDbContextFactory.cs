namespace ECommerceNetApp.Persistence.Implementation.Cart
{
    public interface ICartDbContextFactory
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CartDbContext"/> class.
        /// </summary>
        /// <returns>A new instance of the <see cref="CartDbContext"/> class.</returns>
        CartDbContext CreateDbContext();
    }
}