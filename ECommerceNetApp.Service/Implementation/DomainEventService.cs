using ECommerceNetApp.Domain.Events;
using ECommerceNetApp.Domain.Interfaces;
using MediatR;

namespace ECommerceNetApp.Service.Implementation
{
    public class DomainEventService : IDomainEventService
    {
        private readonly IPublisher _mediator;

        public DomainEventService(IPublisher mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Publish domain event. Process domain event if needed. In a real application, we would publish these events to an event bus.
        /// </summary>
        /// <param name="domainEvent">Domain event.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task completed.</returns>
        public async Task PublishEventAsync(DomainEvent domainEvent, CancellationToken cancellationToken)
        {
            await _mediator.Publish(domainEvent, cancellationToken).ConfigureAwait(false);
        }
    }
}
