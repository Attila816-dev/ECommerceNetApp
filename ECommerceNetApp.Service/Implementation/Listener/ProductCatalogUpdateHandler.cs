using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ECommerceNetApp.Domain.Events;
using ECommerceNetApp.Domain.Events.Product;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerceNetApp.Service.Implementation.Listener
{
    public class ProductCatalogUpdateHandler
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProductCatalogUpdateHandler> _logger;

        public ProductCatalogUpdateHandler(IMediator mediator, ILogger<ProductCatalogUpdateHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task HandleMessageAsync(ServiceBusReceivedMessage message)
        {
            ArgumentNullException.ThrowIfNull(message);

            // Get the event type from metadata
            if (!message.ApplicationProperties.TryGetValue("EventType", out var eventTypeObject)
                || eventTypeObject is not string eventType)
            {
                _logger.LogEventTypeMetadataMissing();
                return;
            }

            // Deserialize based on event type
            DomainEvent? domainEvent = eventType switch
            {
                nameof(ProductCreatedEvent) => JsonSerializer.Deserialize<ProductCreatedEvent>(message.Body.ToString()),
                nameof(ProductUpdatedEvent) => JsonSerializer.Deserialize<ProductUpdatedEvent>(message.Body.ToString()),
                nameof(ProductStockChangedEvent) => JsonSerializer.Deserialize<ProductStockChangedEvent>(message.Body.ToString()),
                nameof(ProductDeletedEvent) => JsonSerializer.Deserialize<ProductStockChangedEvent>(message.Body.ToString()),
                _ => null,
            };

            if (domainEvent == null)
            {
                _logger.LogReceivedUnknownEventType(eventType);
                return;
            }

            // Process the event using MediatR
            await _mediator.Publish(domainEvent).ConfigureAwait(false);
        }
    }
}
