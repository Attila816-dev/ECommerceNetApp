using System.Text.Json;
using ECommerceNetApp.Domain.Events.Category;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerceNetApp.Service.Implementation.NotificationHandlers.Category
{
    public class CategoryUpdatedNotificationHandler(ILogger<CategoryUpdatedNotificationHandler> logger)
        : INotificationHandler<CategoryUpdatedEvent>
    {
        private readonly ILogger<CategoryUpdatedNotificationHandler> _logger = logger;

        public Task Handle(CategoryUpdatedEvent notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            _logger.LogCategoryUpdated(notification.CategoryId, JsonSerializer.Serialize(notification));

            // Add any business logic that should happen when a category is updated
            return Task.CompletedTask;
        }
    }
}
