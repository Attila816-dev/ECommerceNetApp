namespace ECommerceNetApp.Domain.Events.Product
{
    public record ProductUpdatedEvent(int ProductId, string Name, int CategoryId)
        : DomainEvent;
}
