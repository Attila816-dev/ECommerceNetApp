using ECommerceNetApp.Domain.Interfaces;

namespace ECommerceNetApp.Domain.Events.Product
{
    public record ProductDeletedEvent(int ProductId)
        : DomainEvent, IEventBusMessage;
}
