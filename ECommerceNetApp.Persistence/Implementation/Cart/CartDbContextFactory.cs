namespace ECommerceNetApp.Persistence.Implementation.Cart
{
    public class CartDbContextFactory : ICartDbContextFactory
    {
        private readonly string _connectionString;

        public CartDbContextFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public CartDbContext CreateDbContext()
        {
            return new CartDbContext(_connectionString);
        }
    }
}
