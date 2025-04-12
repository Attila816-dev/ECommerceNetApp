using ECommerceNetApp.Domain.Events;
using ECommerceNetApp.Domain.Exceptions;
using ECommerceNetApp.Domain.ValueObjects;

namespace ECommerceNetApp.Domain.Entities
{
    public class Cart
    {
        private readonly List<CartItem> _items = new List<CartItem>();

        // Domain events collection
        private readonly List<DomainEvent> _domainEvents = new List<DomainEvent>();

        public Cart(string id)
        {
            ArgumentException.ThrowIfNullOrEmpty(id);

            Id = id;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cart"/> class.
        /// Default constructor for ORM purposes.
        /// </summary>
        private Cart()
        {
            Id = string.Empty;
        }

        public string Id { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        // Expose items as read-only collection
        public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

        public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        // For event handling/processing
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        public void AddItem(CartItem item)
        {
            ArgumentNullException.ThrowIfNull(item);
            var existingItem = _items.FirstOrDefault(i => i.Id == item.Id);

            if (existingItem != null)
            {
                // Increase quantity of existing item
                int oldQuantity = existingItem.Quantity;
                existingItem.IncreaseQuantity(item.Quantity);
                _domainEvents.Add(new CartItemQuantityUpdatedEvent(Id, item.Id, oldQuantity, existingItem.Quantity));
            }
            else
            {
                // Add new item
                _items.Add(item);
                _domainEvents.Add(new CartItemAddedEvent(Id, item));
            }

            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveItem(int itemId)
        {
            var item = _items.FirstOrDefault(i => i.Id == itemId);

            if (item == null)
            {
                throw new CartItemNotFoundException(itemId);
            }

            _items.Remove(item);
            _domainEvents.Add(new CartItemRemovedEvent(Id, itemId));
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateItemQuantity(int itemId, int newQuantity)
        {
            if (newQuantity <= 0)
            {
                throw new DomainException("Quantity must be greater than zero");
            }

            var item = _items.FirstOrDefault(i => i.Id == itemId);

            if (item == null)
            {
                throw new CartItemNotFoundException(itemId);
            }

            int oldQuantity = item.Quantity;
            item.UpdateQuantity(newQuantity);
            _domainEvents.Add(new CartItemQuantityUpdatedEvent(Id, itemId, oldQuantity, newQuantity));
            UpdatedAt = DateTime.UtcNow;
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
