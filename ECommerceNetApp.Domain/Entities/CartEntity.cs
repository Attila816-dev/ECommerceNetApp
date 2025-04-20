using ECommerceNetApp.Domain.Events.Cart;
using ECommerceNetApp.Domain.Exceptions.Cart;
using ECommerceNetApp.Domain.ValueObjects;

namespace ECommerceNetApp.Domain.Entities
{
    public class CartEntity : BaseEntity
    {
        private readonly List<CartItem> _items = new List<CartItem>();

        public CartEntity(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw InvalidCartException.InvalidCartId();
            }

            Id = id;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CartEntity"/> class.
        /// Default constructor for ORM purposes.
        /// </summary>
        private CartEntity()
        {
            Id = string.Empty;
        }

        public string Id { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        // Expose items as read-only collection
        public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

        public void AddItem(CartItem item)
        {
            ArgumentNullException.ThrowIfNull(item);
            var existingItem = _items.FirstOrDefault(i => i.Id == item.Id);

            if (existingItem != null)
            {
                // Increase quantity of existing item
                int oldQuantity = existingItem.Quantity;
                existingItem.IncreaseQuantity(item.Quantity);
                AddDomainEvent(new CartItemQuantityUpdatedEvent(Id, item.Id, oldQuantity, existingItem.Quantity));
            }
            else
            {
                // Add new item
                _items.Add(item);
                AddDomainEvent(new CartItemAddedEvent(Id, item));
            }

            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveItem(int itemId)
        {
            var item = _items.FirstOrDefault(i => i.Id == itemId);

            if (item == null)
            {
                throw InvalidCartException.CartItemNotFound(itemId);
            }

            _items.Remove(item);
            AddDomainEvent(new CartItemRemovedEvent(Id, itemId));
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateItemQuantity(int itemId, int newQuantity)
        {
            if (newQuantity <= 0)
            {
                throw InvalidCartException.InvalidCartItemQuantity();
            }

            var item = _items.FirstOrDefault(i => i.Id == itemId);

            if (item == null)
            {
                throw InvalidCartException.CartItemNotFound(itemId);
            }

            int oldQuantity = item.Quantity;
            item.UpdateQuantity(newQuantity);
            AddDomainEvent(new CartItemQuantityUpdatedEvent(Id, itemId, oldQuantity, newQuantity));
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsDeleted()
        {
            AddDomainEvent(new CartDeletedEvent(Id));
        }

        public Money CalculateTotal()
        {
            return _items
                .Where(i => i.TotalPrice != null)
                .Aggregate(
                    Money.From(0),
                    (total, item) => total + item.TotalPrice!);
        }
    }
}
