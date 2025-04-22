using ECommerceNetApp.Domain.Events.Category;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerceNetApp.Service.Implementation.NotificationHandlers.Category
{
    public class CategoryDeletedNotificationHandler(ILogger<CategoryDeletedNotificationHandler> logger)
        : INotificationHandler<CategoryDeletedEvent>
    {
        private readonly ILogger<CategoryDeletedNotificationHandler> _logger = logger;

        public Task Handle(CategoryDeletedEvent notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            _logger.LogCategoryDeleted(notification.CategoryId);

            // Add any business logic that should happen when a category is deleted
            return Task.CompletedTask;
        }
    }
}
