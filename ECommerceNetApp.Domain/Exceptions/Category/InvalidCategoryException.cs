using System.Net;
using ECommerceNetApp.Domain.Entities;

namespace ECommerceNetApp.Domain.Exceptions.Category
{
    public class InvalidCategoryException : DomainException
    {
        public InvalidCategoryException(string message)
            : base(message)
        {
        }

        public InvalidCategoryException(string message, HttpStatusCode statusCode)
            : base(message, statusCode)
        {
        }

        public InvalidCategoryException()
        {
        }

        public InvalidCategoryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public static InvalidCategoryException NameRequired()
        {
            return new InvalidCategoryException("Category name is required");
        }

        public static InvalidCategoryException NameTooLong()
        {
            return new InvalidCategoryException($"Category name cannot exceed {CategoryEntity.MaxCategoryNameLength} characters");
        }

        public static InvalidCategoryException CircularReference()
        {
            return new InvalidCategoryException("Circular reference detected in category hierarchy");
        }

        public static InvalidCategoryException NotFound(int id)
        {
            return new InvalidCategoryException($"Category with id {id} not found", HttpStatusCode.NotFound);
        }

        public static InvalidCategoryException HasSubCategories()
        {
            return new InvalidCategoryException("Cannot delete category with subCategories");
        }

        public static InvalidCategoryException HasProducts()
        {
            return new InvalidCategoryException("Cannot delete category with associated products");
        }
    }
}
