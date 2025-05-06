using ECommerceNetApp.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceNetApp.Service.Implementation
{
    public class SimplePublisher(IServiceProvider serviceProvider) : IPublisher
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        public async Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            var notificationHandlers = _serviceProvider.GetServices<INotificationHandler<TNotification>>();

            foreach (var notificationHandler in notificationHandlers)
            {
                await notificationHandler.HandleAsync(notification, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
