namespace ECommerceNetApp.Domain.Events.Product
{
    public record ProductDeletedEvent(int ProductId)
        : DomainEvent;
}
