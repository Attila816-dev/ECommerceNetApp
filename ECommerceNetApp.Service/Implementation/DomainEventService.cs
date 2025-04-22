using ECommerceNetApp.Domain.Events;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Service.Interfaces.Publisher;
using MediatR;

namespace ECommerceNetApp.Service.Implementation
{
    public class DomainEventService(IPublisher mediator, IServiceBusPublisher serviceBusPublisher)
        : IDomainEventService
    {
#pragma warning disable CA1805 // Do not initialize unnecessarily
        private readonly bool useServiceBus = false; // Set to true if you want to use Azure Service Bus
#pragma warning restore CA1805 // Do not initialize unnecessarily
        private readonly IPublisher _mediator = mediator;
        private readonly IServiceBusPublisher _serviceBusPublisher = serviceBusPublisher;

        /// <summary>
        /// Publish domain event. Process domain event if needed. In a real application, we would publish these events to an event bus.
        /// </summary>
        /// <param name="domainEvent">Domain event.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task completed.</returns>
        public async Task PublishEventAsync(DomainEvent domainEvent, CancellationToken cancellationToken)
        {
            // Publish to Azure Service Bus
            if (useServiceBus && domainEvent is IEventBusMessage)
            {
                await _serviceBusPublisher.PublishMessageAsync(domainEvent, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // Publish to MediatR as fallback
                await _mediator.Publish(domainEvent, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
