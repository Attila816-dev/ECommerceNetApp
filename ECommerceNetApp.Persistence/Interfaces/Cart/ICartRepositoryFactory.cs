namespace ECommerceNetApp.Persistence.Interfaces.Cart
{
    public interface ICartRepositoryFactory
    {
        ICartRepository CreateRepository(ICartUnitOfWork unitOfWork);
    }
}
