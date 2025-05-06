using ECommerceNetApp.Domain.Events.Category;
using ECommerceNetApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerceNetApp.Service.Implementation.NotificationHandlers.Category
{
    public class CategoryDeletedNotificationHandler(ILogger<CategoryDeletedNotificationHandler> logger)
        : INotificationHandler<CategoryDeletedEvent>
    {
        private static readonly Action<ILogger, int, Exception?> LogCategoryDeleted =
            LoggerMessage.Define<int>(
                LogLevel.Information,
                new EventId(1, nameof(CategoryDeletedNotificationHandler)),
                "Category deleted: {CategoryId}.");

        private readonly ILogger<CategoryDeletedNotificationHandler> _logger = logger;

        public Task HandleAsync(CategoryDeletedEvent notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            LogCategoryDeleted(_logger, notification.CategoryId, null);

            // Add any business logic that should happen when a category is deleted
            return Task.CompletedTask;
        }
    }
}
