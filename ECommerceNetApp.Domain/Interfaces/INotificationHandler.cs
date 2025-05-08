namespace ECommerceNetApp.Domain.Interfaces
{
    /// <summary>
    /// Defines a handler for a notification.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being handled.</typeparam>
    public interface INotificationHandler<in TNotification>
        where TNotification : INotification
    {
        /// <summary>
        /// Registers the handler with the event bus.
        /// </summary>
        /// <param name="eventBus">The event bus.</param>
        void Register(IEventBus eventBus);

        /// <summary>
        /// Handles a notification.
        /// </summary>
        /// <param name="notification">The notification.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task HandleAsync(TNotification notification, CancellationToken cancellationToken);
    }
}
