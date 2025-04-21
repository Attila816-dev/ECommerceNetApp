using System.Net;
using ECommerceNetApp.Domain.Entities;

namespace ECommerceNetApp.Domain.Exceptions.Product
{
    public class InvalidProductException : DomainException
    {
        public InvalidProductException(string message)
            : base(message)
        {
        }

        public InvalidProductException(string message, HttpStatusCode statusCode)
            : base(message, statusCode)
        {
        }

        public InvalidProductException()
        {
        }

        public InvalidProductException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public static InvalidProductException NameRequired()
        {
            return new InvalidProductException("Product name is required");
        }

        public static InvalidProductException NameTooLong()
        {
            return new InvalidProductException($"Product name cannot exceed {ProductEntity.MaxProductNameLength} characters");
        }

        public static InvalidProductException CategoryRequired()
        {
            return new InvalidProductException("Product category is required");
        }

        public static InvalidProductException InvalidPrice()
        {
            return new InvalidProductException("Price cannot be negative");
        }

        public static InvalidProductException InvalidAmount()
        {
            return new InvalidProductException("Amount must be a positive integer");
        }

        public static InvalidProductException NotFound(Guid id)
        {
            return new InvalidProductException($"Product with id {id} not found", HttpStatusCode.NotFound);
        }
    }
}
