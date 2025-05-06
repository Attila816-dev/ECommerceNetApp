using System.Text.Json;
using ECommerceNetApp.Domain.Events.Category;
using ECommerceNetApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerceNetApp.Service.Implementation.NotificationHandlers.Category
{
    public class CategoryUpdatedNotificationHandler(ILogger<CategoryUpdatedNotificationHandler> logger)
        : INotificationHandler<CategoryUpdatedEvent>
    {
        // Define a static LoggerMessage delegate for improved performance and consistent templates
        private static readonly Action<ILogger, int, string, Exception?> LogCategoryCreated =
            LoggerMessage.Define<int, string>(
                LogLevel.Information,
                new EventId(1, nameof(CategoryUpdatedNotificationHandler)),
                "Category {CategoryId} updated with properties: {SerializedNotification}");

        private readonly ILogger<CategoryUpdatedNotificationHandler> _logger = logger;

        public Task HandleAsync(CategoryUpdatedEvent notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);

            LogCategoryCreated(
                _logger,
                notification.CategoryId,
                JsonSerializer.Serialize(notification),
                null);

            // Add any business logic that should happen when a category is updated
            return Task.CompletedTask;
        }
    }
}
