using ECommerceNetApp.Domain.Interfaces;

namespace ECommerceNetApp.Service.Implementation.EventBus
{
    internal sealed class InMemoryEventBus(InMemoryMessageQueue queue) : IEventBus
    {
        private readonly Dictionary<Type, List<Func<INotification, CancellationToken, Task>>> _handlers = new();
        private readonly InMemoryMessageQueue _queue = queue;

        public void Register<TNotification>(INotificationHandler<TNotification> handler)
            where TNotification : INotification
        {
            var eventType = typeof(TNotification);

            if (!_handlers.TryGetValue(eventType, out var handlerList))
            {
                handlerList = new();
                _handlers[eventType] = handlerList;
            }

            handlerList.Add((evt, token) => handler.HandleAsync((TNotification)evt, token));
        }

        public async Task PublishAsync<TNotification>(
            TNotification integrationEvent,
            CancellationToken cancellationToken = default)
            where TNotification : class, INotification
        {
            await _queue.Writer.WriteAsync(integrationEvent, cancellationToken).ConfigureAwait(false);
        }

        public async Task StartConsumingAsync(CancellationToken cancellationToken = default)
        {
            await foreach (var integrationEvent in _queue.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
            {
                if (!_handlers.TryGetValue(integrationEvent.GetType(), out var handlers))
                {
                    continue;
                }

                foreach (var handler in handlers)
                {
                    await handler(integrationEvent, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
