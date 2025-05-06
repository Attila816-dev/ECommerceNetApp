namespace ECommerceNetApp.Domain.Interfaces
{
    public interface IPublisher
    {
        /// <summary>
        /// Asynchronously send a notification to multiple handlers.
        /// </summary>
        /// <param name="notification">Notification object.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <typeparam name="TNotification">Notification type.</typeparam>
        /// <returns>A task that represents the publish operation.</returns>
        Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification;
    }
}
