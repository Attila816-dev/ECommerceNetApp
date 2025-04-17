namespace ECommerceNetApp.Domain.Events.Product
{
    public record ProductStockChangedEvent(int ProductId, int NewAmount, int OldAmount)
        : DomainEvent;
}
