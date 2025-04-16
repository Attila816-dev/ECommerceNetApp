namespace ECommerceNetApp.Domain.Exceptions.Cart
{
    public class CartItemNotFoundException : DomainException
    {
        public CartItemNotFoundException(int itemId)
            : base($"Item with ID {itemId} not found in cart")
        {
        }

        public CartItemNotFoundException()
        {
        }

        public CartItemNotFoundException(string message)
            : base(message)
        {
        }

        public CartItemNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
