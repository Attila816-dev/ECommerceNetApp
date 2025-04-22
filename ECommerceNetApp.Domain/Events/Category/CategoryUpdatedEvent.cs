using ECommerceNetApp.Domain.Interfaces;

namespace ECommerceNetApp.Domain.Events.Category
{
    public record CategoryUpdatedEvent(
        int CategoryId,
        string Name,
#pragma warning disable CA1054 // URI-like parameters should not be strings
        string? ImageUrl,
#pragma warning restore CA1054 // URI-like parameters should not be strings
        int? ParentCategoryId) : DomainEvent, IEventBusMessage;
}
