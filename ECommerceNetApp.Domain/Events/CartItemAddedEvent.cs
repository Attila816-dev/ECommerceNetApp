using ECommerceNetApp.Domain.ValueObjects;

namespace ECommerceNetApp.Domain.Events
{
    public class CartItemAddedEvent : DomainEvent
    {
        public CartItemAddedEvent(string cartId, CartItem item)
        {
            CartId = cartId;
            Item = item;
        }

        public string CartId { get; }

        public CartItem Item { get; }
    }
}
