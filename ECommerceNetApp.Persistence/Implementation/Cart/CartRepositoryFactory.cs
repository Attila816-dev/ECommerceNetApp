using ECommerceNetApp.Persistence.Interfaces.Cart;

namespace ECommerceNetApp.Persistence.Implementation.Cart
{
    public class CartRepositoryFactory : ICartRepositoryFactory
    {
        private readonly CartDbContext _dbContext;

        public CartRepositoryFactory(CartDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public ICartRepository CreateRepository(ICartUnitOfWork unitOfWork)
        {
            return new CartRepository(_dbContext, unitOfWork);
        }
    }
}
