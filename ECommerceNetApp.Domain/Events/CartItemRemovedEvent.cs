namespace ECommerceNetApp.Domain.Events
{
    public class CartItemRemovedEvent : DomainEvent
    {
        public CartItemRemovedEvent(string cartId, int itemId)
        {
            CartId = cartId;
            ItemId = itemId;
        }

        public string CartId { get; }

        public int ItemId { get; }
    }
}
