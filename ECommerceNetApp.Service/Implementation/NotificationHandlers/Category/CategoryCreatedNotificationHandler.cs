using System.Text.Json;
using ECommerceNetApp.Domain.Events.Category;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerceNetApp.Service.Implementation.NotificationHandlers.Category
{
    public class CategoryCreatedNotificationHandler(ILogger<CategoryCreatedNotificationHandler> logger)
        : INotificationHandler<CategoryCreatedEvent>
    {
        private readonly ILogger<CategoryCreatedNotificationHandler> _logger = logger;

        public Task Handle(CategoryCreatedEvent notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            _logger.LogCategoryCreated(notification.CategoryId, JsonSerializer.Serialize(notification));

            // Add any business logic that should happen when a category is created
            // For example, you could send an email to administrators, update search indexes, etc.
            return Task.CompletedTask;
        }
    }
}
