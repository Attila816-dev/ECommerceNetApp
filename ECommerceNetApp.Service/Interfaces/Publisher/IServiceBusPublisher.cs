namespace ECommerceNetApp.Service.Interfaces.Publisher
{
    public interface IServiceBusPublisher
    {
        Task PublishMessageAsync<T>(T message, CancellationToken cancellationToken = default);
    }
}