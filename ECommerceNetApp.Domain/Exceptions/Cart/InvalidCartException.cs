namespace ECommerceNetApp.Domain.Exceptions.Cart
{
    public class InvalidCartException : DomainException
    {
        public InvalidCartException(string message)
        : base(message)
        {
        }

        public InvalidCartException()
        {
        }

        public InvalidCartException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public static Exception CartNotFound(string cartId) =>
            new InvalidCartException($"Cart with ID '{cartId}' was not found.");

        public static Exception CartItemNotFound(int itemId) =>
            new InvalidCartException($"Cart item with ID '{itemId}' was not found.");

        public static Exception InvalidCartId() =>
            new InvalidCartException("Cart ID cannot be null or empty.");

        public static Exception InvalidCartItemQuantity() =>
            new InvalidCartException("Cart item quantity must be greater than zero.");
    }
}
