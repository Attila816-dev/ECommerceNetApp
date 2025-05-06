using ECommerceNetApp.Domain.Events;
using ECommerceNetApp.Domain.Interfaces;

namespace ECommerceNetApp.Service.Implementation
{
    public class DomainEventService : IDomainEventService
    {
        private readonly IPublisher _publisher;

        public DomainEventService(IPublisher publisher)
        {
            _publisher = publisher;
        }

        /// <summary>
        /// Publish domain event. Process domain event if needed. In a real application, we would publish these events to an event bus.
        /// </summary>
        /// <param name="domainEvent">Domain event.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task completed.</returns>
        public async Task PublishEventAsync(DomainEvent domainEvent, CancellationToken cancellationToken)
        {
            await _publisher.PublishAsync(domainEvent, cancellationToken).ConfigureAwait(false);
        }
    }
}
