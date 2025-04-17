namespace ECommerceNetApp.Domain.Events.Cart
{
    public record CartItemQuantityUpdatedEvent(string CartId, int CartItemId, int OldQuantity, int NewQuantity)
        : DomainEvent;
}
