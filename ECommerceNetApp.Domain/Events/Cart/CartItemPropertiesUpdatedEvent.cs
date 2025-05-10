using ECommerceNetApp.Domain.ValueObjects;

namespace ECommerceNetApp.Domain.Events.Cart
{
    public record CartItemPropertiesUpdatedEvent(string CartId, CartItem CartItem)
        : DomainEvent;
}
