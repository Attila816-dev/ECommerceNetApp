using ECommerceNetApp.Domain.Events;
using ECommerceNetApp.Domain.Interfaces;

namespace ECommerceNetApp.Service.Implementation
{
    public class DomainEventService : IDomainEventService
    {
        private readonly IEventBus _eventBus;

        public DomainEventService(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        /// <summary>
        /// Publish domain event. Process domain event if needed. In a real application, we would publish these events to an event bus.
        /// </summary>
        /// <param name="domainEvent">Domain event.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task completed.</returns>
        public async Task PublishEventAsync(DomainEvent domainEvent, CancellationToken cancellationToken)
        {
            await _eventBus.PublishAsync(domainEvent, cancellationToken).ConfigureAwait(false);
        }
    }
}
