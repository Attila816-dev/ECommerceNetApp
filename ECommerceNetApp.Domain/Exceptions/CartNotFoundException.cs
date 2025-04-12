namespace ECommerceNetApp.Domain.Exceptions
{
    public class CartNotFoundException : DomainException
    {
        public CartNotFoundException(string cartId)
            : base($"Cart with ID {cartId} not found")
        {
        }

        public CartNotFoundException()
        {
        }

        public CartNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
