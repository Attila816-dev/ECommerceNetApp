using ECommerceNetApp.Domain.ValueObjects;

namespace ECommerceNetApp.Domain.Events.Category
{
    public record CategoryUpdatedEvent(
        int CategoryId,
        string Name,
        ImageInfo? Image,
        int? ParentCategoryId) : DomainEvent;
}
