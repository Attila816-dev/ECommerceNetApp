using ECommerceNetApp.Domain.Interfaces;

namespace ECommerceNetApp.Domain.Events.Cart
{
    public record CartItemRemovedEvent(string CartId, int CartItemId) : DomainEvent, IEventBusMessage;
}
