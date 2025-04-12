using ECommerceNetApp.Domain.Exceptions;

namespace ECommerceNetApp.Domain.ValueObjects
{
    public class CartItem : IEquatable<CartItem>
    {
        public CartItem(int id, string? name, Money price, int quantity, ImageInfo? image = null)
        {
            if (id <= 0)
            {
                throw new DomainException("Item ID must be positive");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException("Item name cannot be empty");
            }

            if (quantity <= 0)
            {
                throw new DomainException("Item quantity must be positive");
            }

            Id = id;
            Name = name;
            Price = price ?? throw new DomainException("Price cannot be null");
            Quantity = quantity;
            Image = image;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CartItem"/> class.
        /// Default constructor for ORM purposes.
        /// </summary>
        private CartItem()
        {
        }

        public int Id { get; private set; }

        public string? Name { get; private set; }

        public ImageInfo? Image { get; private set; }

        public Money? Price { get; private set; }

        public int Quantity { get; private set; }

        public Money? TotalPrice
        {
            get
            {
                if (Price == null)
                {
                    return null;
                }

                return Price * Quantity;
            }
        }

        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
            {
                throw new DomainException("Quantity must be positive");
            }

            Quantity = newQuantity;
        }

        public void IncreaseQuantity(int additionalQuantity)
        {
            if (additionalQuantity <= 0)
            {
                throw new DomainException("Additional quantity must be positive");
            }

            Quantity += additionalQuantity;
        }

        public bool Equals(CartItem? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as CartItem);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
