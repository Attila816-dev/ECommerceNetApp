namespace ECommerceNetApp.Domain.Events.Category
{
    public record CategoryDeletedEvent(int CategoryId) : DomainEvent;
}
