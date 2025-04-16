using ECommerceNetApp.Domain.ValueObjects;

namespace ECommerceNetApp.Domain.Events.Cart
{
    public record CartItemAddedEvent(string CartId, CartItem CartItem) : DomainEvent;
}
