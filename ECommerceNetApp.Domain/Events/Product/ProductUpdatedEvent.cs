using ECommerceNetApp.Domain.Interfaces;

namespace ECommerceNetApp.Domain.Events.Product
{
    public record ProductUpdatedEvent(int ProductId,
        string Name,
        string? Description,
        int CategoryId,
#pragma warning disable CA1054 // URI-like parameters should not be strings
        string? ImageUrl,
#pragma warning restore CA1054 // URI-like parameters should not be strings
        decimal Price,
        int Amount)
        : DomainEvent, IEventBusMessage;
}
