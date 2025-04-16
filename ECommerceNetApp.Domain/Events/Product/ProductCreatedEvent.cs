namespace ECommerceNetApp.Domain.Events.Product
{
    public record ProductCreatedEvent(int ProductId, string Name, int CategoryId)
        : DomainEvent;
}
