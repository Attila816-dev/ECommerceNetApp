using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.ValueObjects;

namespace ECommerceNetApp.Domain.Events.Product
{
    public record ProductUpdatedEvent(int ProductId,
        string Name,
        string? Description,
        int CategoryId,
#pragma warning disable CA1054 // URI-like parameters should not be strings
        ImageInfo? ImageUrl,
#pragma warning restore CA1054 // URI-like parameters should not be strings
        Money Price,
        int Amount)
        : DomainEvent, IEventBusMessage;
}
