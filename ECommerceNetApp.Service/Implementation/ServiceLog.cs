using Microsoft.Extensions.Logging;

namespace ECommerceNetApp.Service.Implementation
{
    /// <summary>
    /// Provides logging methods for the ECommerceNetApp services.
    /// </summary>
    public static partial class ServiceLog
    {
        /// <summary>
        /// Logs a warning when the EventType metadata is missing.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance used to log the message.</param>
        [LoggerMessage(EventId = 0, Level = LogLevel.Warning, Message = "EventType metadata is missing.")]
        public static partial void LogEventTypeMetadataMissing(this ILogger logger);

        /// <summary>
        /// Logs an error when an event couldn't be deserialized or an unknown event type is encountered.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance used to log the message.</param>
        /// <param name="eventType">The unknown event type that caused the error.</param>
        [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Failed to deserialize or unknown event type: {EventType}")]
        public static partial void LogReceivedUnknownEventType(this ILogger logger, string eventType);

        /// <summary>
        /// Logs an informational message when a product is created.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance used to log the message.</param>
        /// <param name="productId">The ID of the created product.</param>
        /// <param name="properties">The properties of the created product.</param>
        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Product {ProductId} created with properties: {Properties}")]
        public static partial void LogProductCreated(this ILogger logger, int productId, string properties);

        /// <summary>
        /// Logs an informational message when a product is updated.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance used to log the message.</param>
        /// <param name="productId">The ID of the updated product.</param>
        /// <param name="properties">The updated properties of the product.</param>
        [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Product {ProductId} updated with properties: {Properties}")]
        public static partial void LogProductUpdated(this ILogger logger, int productId, string properties);

        /// <summary>
        /// Logs an informational message when a product is deleted.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance used to log the message.</param>
        /// <param name="productId">The ID of the deleted product.</param>
        [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Product deleted: {ProductId}")]
        public static partial void LogProductDeleted(this ILogger logger, int productId);

        /// <summary>
        /// Logs an informational message when the stock of a product changes.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance used to log the message.</param>
        /// <param name="productId">The ID of the product whose stock changed.</param>
        /// <param name="oldAmount">The previous stock amount.</param>
        /// <param name="newAmount">The new stock amount.</param>
        [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Product stock changed: {ProductId}, Old: {OldAmount}, New: {NewAmount}")]
        public static partial void LogProductStockChanged(this ILogger logger, int productId, int oldAmount, int newAmount);

        /// <summary>
        /// Logs an informational message when a category is created.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance used to log the message.</param>
        /// <param name="categoryId">The ID of the created category.</param>
        /// <param name="properties">The properties of the created category.</param>
        [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "Category {CategoryId} created with properties: {Properties}")]
        public static partial void LogCategoryCreated(this ILogger logger, int categoryId, string properties);

        /// <summary>
        /// Logs an informational message when a category is updated.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance used to log the message.</param>
        /// <param name="categoryId">The ID of the updated category.</param>
        /// <param name="properties">The updated properties of the category.</param>
        [LoggerMessage(EventId = 7, Level = LogLevel.Information, Message = "Category {CategoryId} updated with properties: {Properties}")]
        public static partial void LogCategoryUpdated(this ILogger logger, int categoryId, string properties);

        /// <summary>
        /// Logs an informational message when a category is deleted.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance used to log the message.</param>
        /// <param name="categoryId">The ID of the deleted category.</param>
        [LoggerMessage(EventId = 8, Level = LogLevel.Information, Message = "Category deleted: {CategoryId}")]
        public static partial void LogCategoryDeleted(this ILogger logger, int categoryId);

        /// <summary>
        /// Logs an informational message when a cart is deleted.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance used to log the message.</param>
        /// <param name="cartId">The ID of the deleted cart.</param>
        [LoggerMessage(EventId = 9, Level = LogLevel.Information, Message = "Cart deleted: {CartId}")]
        public static partial void LogCartDeleted(this ILogger logger, string cartId);

        /// <summary>
        /// Logs an informational message when a cart item is added.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance used to log the message.</param>
        /// <param name="cartId">The ID of the cart.</param>
        /// <param name="properties">The properties of the cart item.</param>
        [LoggerMessage(EventId = 10, Level = LogLevel.Information, Message = "Cart item added to cart {CartId} with properties: {Properties}")]
        public static partial void LogCartItemAdded(this ILogger logger, string cartId, string properties);

        /// <summary>
        /// Logs an informational message when a cart item is removed from cart.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance used to log the message.</param>
        /// <param name="cartId">The Id of the cart.</param>
        /// <param name="itemId">The Id of the cart item.</param>
        [LoggerMessage(EventId = 11, Level = LogLevel.Information, Message = "Cart item removed from cart {CartId} with id: {ItemId}")]
        public static partial void LogCartItemRemoved(this ILogger logger, string cartId, int itemId);

        /// <summary>
        /// Logs an error when an issue occurs while migrating or seeding the database.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance used to log the message.</param>
        /// <param name="exception">The exception that occurred.</param>
        [LoggerMessage(EventId = 12, Level = LogLevel.Error, Message = "An error occurred while migrating or seeding the database.")]
        public static partial void LogSeedDatabaseError(this ILogger logger, Exception exception);

        /// <summary>
        /// Logs an error when an unexpected issue occurs while listening to the EventBus.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance used to log the message.</param>
        /// <param name="exception">The exception that occurred.</param>
        [LoggerMessage(EventId = 13, Level = LogLevel.Error, Message = "An error occurred during listening to EventBus.")]
        public static partial void LogUnexpectedEventBusListeningError(this ILogger logger, Exception exception);
    }
}
