using ECommerceNetApp.Domain.Events.Cart;
using ECommerceNetApp.Domain.Exceptions.Cart;
using ECommerceNetApp.Domain.ValueObjects;

namespace ECommerceNetApp.Domain.Entities
{
    public class CartEntity : BaseEntity<string>
    {
        private readonly List<CartItem> _items = new List<CartItem>();

        internal CartEntity(string id)
            : base(id)
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;

            // AddDomainEvent(new CartCreatedEvent(id));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CartEntity"/> class.
        /// Default constructor for ORM purposes.
        /// </summary>
        private CartEntity()
            : base(string.Empty)
        {
        }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        // Expose items as read-only collection
        public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

        public static CartEntity Create(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw InvalidCartException.InvalidCartId();
            }

            return new CartEntity(id);
        }

        public void AddItem(int id, string name, Money price, int quantity, ImageInfo? image = null)
        {
            var cartItem = CartItem.Create(id, name, price, quantity, image);

            var itemIndex = _items.FindIndex(i => i.Id == cartItem.Id);
            if (itemIndex < 0)
            {
                // Add new item
                _items.Add(cartItem);
                AddDomainEvent(new CartItemAddedEvent(Id, cartItem));
            }
            else
            {
                // Increase quantity of existing item
                var existingItem = _items[itemIndex];
                int oldQuantity = existingItem.Quantity;

                // Replace with new immutable item
                var updatedItem = existingItem.WithIncreasedQuantity(cartItem.Quantity);
                _items[itemIndex] = updatedItem;

                AddDomainEvent(new CartItemQuantityUpdatedEvent(Id, cartItem.Id, oldQuantity, existingItem.Quantity));
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

            var itemIndex = _items.FindIndex(i => i.Id == itemId);
            if (itemIndex < 0)
            {
                throw InvalidCartException.CartItemNotFound(itemId);
            }

            var oldItem = _items[itemIndex];
            int oldQuantity = oldItem.Quantity;

            // Replace with new immutable item
            var updatedItem = oldItem.WithUpdatedQuantity(newQuantity);
            _items[itemIndex] = updatedItem;

            AddDomainEvent(new CartItemQuantityUpdatedEvent(Id, itemId, oldQuantity, newQuantity));
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateItemProperties(int itemId, string newName, Money newPrice, ImageInfo? newImage = null)
        {
            var itemIndex = _items.FindIndex(i => i.Id == itemId);
            if (itemIndex < 0)
            {
                return; // Item not in cart
            }

            var oldItem = _items[itemIndex];
            var updatedItem = oldItem.WithUpdatedProperties(newName, newPrice, newImage);
            _items[itemIndex] = updatedItem;

            // Raise domain event for audit/tracking
            AddDomainEvent(new CartItemPropertiesUpdatedEvent(Id, updatedItem));
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveAllItems()
        {
            foreach (var item in _items.ToList())
            {
                RemoveItem(item.Id);
            }
        }

        public override void MarkAsDeleted()
        {
            AddDomainEvent(new CartDeletedEvent(Id));
        }

        public Money CalculateTotal()
        {
            if (Items.Count == 0 || _items.All(i => i.TotalPrice == null))
            {
                return Money.From(0);
            }

            return _items
                .Where(i => i.TotalPrice != null)
                .Aggregate(
                    Money.From(0),
                    (total, item) => total + item.TotalPrice!);
        }
    }
}
