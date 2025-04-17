using ECommerceNetApp.Domain.Entities;
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
        /// Publish domain events for the given entity. Process domain events if needed. In a real application, we would publish these events to an event bus.
        /// </summary>
        /// <param name="entity">Domain entity.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task completed.</returns>
        public async Task PublishEventsAsync(BaseEntity entity, CancellationToken cancellationToken)
        {
            if (entity?.DomainEvents == null || entity.DomainEvents.Count == 0)
            {
                return;
            }

            foreach (var domainEvent in entity.DomainEvents)
            {
                await _mediator.Publish(domainEvent, cancellationToken).ConfigureAwait(false);
            }

            entity.ClearDomainEvents();
        }
    }
}
