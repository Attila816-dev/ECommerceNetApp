namespace ECommerceNetApp.Persistence.Interfaces.Cart
{
    public interface ICartUnitOfWork : IDisposable
    {
        ICartRepository CartRepository { get; }

        Task CommitAsync(CancellationToken cancellationToken = default);
    }
}
