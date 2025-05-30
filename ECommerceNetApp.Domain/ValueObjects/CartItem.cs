using ECommerceNetApp.Domain.Exceptions.Cart;

namespace ECommerceNetApp.Domain.ValueObjects
{
    public record CartItem
    {
        private CartItem(int id, string name, Money price, int quantity, ImageInfo? image = null)
        {
            Id = id;
            Name = name;
            Price = price;
            Quantity = quantity;
            Image = image;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CartItem"/> class.
        /// Default constructor for ORM purposes.
        /// </summary>
        private CartItem()
            : this(default, string.Empty, Money.From(0), 0)
        {
        }

        public int Id { get; init; }

        public string Name { get; init; }

        public ImageInfo? Image { get; init; }

        public Money Price { get; init; }

        public int Quantity { get; private set; }

        public Money TotalPrice => Price * Quantity;

        internal static CartItem Create(int id, string name, Money price, int quantity, ImageInfo? image = null)
        {
            if (id <= 0)
            {
                throw InvalidCartException.InvalidCartItemId();
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw InvalidCartException.InvalidCartItemName();
            }

            if (quantity <= 0)
            {
                throw InvalidCartException.InvalidCartItemQuantity();
            }

            ArgumentNullException.ThrowIfNull(price, nameof(price));

            return new CartItem(id, name, price, quantity, image);
        }

        // Since we need to modify Quantity, we can't make it init-only
        // Instead, we need methods to create a new instance with updated values
        public CartItem WithUpdatedQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
            {
                throw InvalidCartException.InvalidCartItemQuantity();
            }

            // Use the with expression to create a new record with updated quantity
            return this with { Quantity = newQuantity };
        }

        public CartItem WithUpdatedProperties(string newName, Money newPrice, ImageInfo? newImage = null)
        {
            return this with { Price = newPrice, Name = newName, Image = newImage ?? Image };
        }

        public CartItem WithIncreasedQuantity(int additionalQuantity)
        {
            if (additionalQuantity <= 0)
            {
                throw InvalidCartException.InvalidCartItemQuantity();
            }

            // Use the with expression to create a new record with updated quantity
            return this with { Quantity = Quantity + additionalQuantity };
        }
    }
}