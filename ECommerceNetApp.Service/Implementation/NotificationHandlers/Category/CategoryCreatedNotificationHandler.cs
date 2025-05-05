using System.Text.Json;
using ECommerceNetApp.Domain.Events.Category;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerceNetApp.Service.Implementation.NotificationHandlers.Category
{
    public class CategoryCreatedNotificationHandler(ILogger<CategoryCreatedNotificationHandler> logger)
        : INotificationHandler<CategoryCreatedEvent>
    {
        // Define a static LoggerMessage delegate for improved performance and consistent templates
        private static readonly Action<ILogger, string, Exception?> LogCategoryCreated =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(1, nameof(CategoryCreatedNotificationHandler)),
                "Category created with Properties: {SerializedNotification}");

        private readonly ILogger<CategoryCreatedNotificationHandler> _logger = logger;

        public Task Handle(CategoryCreatedEvent notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            LogCategoryCreated(_logger, JsonSerializer.Serialize(notification), null);

            // Add any business logic that should happen when a category is created
            // For example, you could send an email to administrators, update search indexes, etc.
            return Task.CompletedTask;
        }
    }
}
