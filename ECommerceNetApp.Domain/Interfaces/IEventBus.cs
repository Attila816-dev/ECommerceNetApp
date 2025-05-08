namespace ECommerceNetApp.Domain.Interfaces
{
    public interface IEventBus
    {
        void Register<TNotification>(INotificationHandler<TNotification> handler)
           where TNotification : INotification;

        Task PublishAsync<TNotification>(
           TNotification integrationEvent,
           CancellationToken cancellationToken = default)
           where TNotification : class, INotification;

        Task StartConsumingAsync(CancellationToken cancellationToken = default);
    }
}
