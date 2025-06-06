﻿using System.Net;

namespace ECommerceNetApp.Domain.Exceptions.Cart
{
    public class InvalidCartException : DomainException
    {
        public InvalidCartException(string message)
        : base(message)
        {
        }

        public InvalidCartException(string message, HttpStatusCode statusCode)
            : base(message, statusCode)
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
            new InvalidCartException($"Cart with ID '{cartId}' was not found.", HttpStatusCode.NotFound);

        public static Exception CartItemNotFound(int itemId) =>
            new InvalidCartException($"Cart item with ID '{itemId}' was not found.", HttpStatusCode.NotFound);

        public static Exception InvalidCartId() =>
            new InvalidCartException("Cart ID cannot be null or empty.");

        public static Exception InvalidCartItemQuantity() =>
            new InvalidCartException("Cart item quantity must be greater than zero.");

        public static Exception InvalidCartItemId() =>
            new InvalidCartException("Cart item id must be greater than zero.");

        public static Exception InvalidCartItemName() =>
            new InvalidCartException("Cart item name cannot be null or empty.");

        public static Exception InvalidCartItemPrice() =>
            new InvalidCartException("Cart item price cannot be null.");

        public static Exception InvalidProduct() =>
            new InvalidCartException("Cart product cannot be null.");
    }
}
