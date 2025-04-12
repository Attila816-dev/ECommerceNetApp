using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerceNetApp.Domain.ValueObjects;

namespace ECommerceNetApp.Domain.Events
{
    public class CartItemQuantityUpdatedEvent : DomainEvent
    {
        public CartItemQuantityUpdatedEvent(string cartId, int itemId, int oldQuantity, int newQuantity)
        {
            CartId = cartId;
            ItemId = itemId;
            OldQuantity = oldQuantity;
            NewQuantity = newQuantity;
        }

        public string CartId { get; }

        public int ItemId { get; }

        public int OldQuantity { get; }

        public int NewQuantity { get; }
    }
}
