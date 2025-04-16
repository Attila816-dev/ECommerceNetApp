namespace ECommerceNetApp.Domain.Events.Category
{
    public record CategoryCreatedEvent(int CategoryId, string Name) : DomainEvent;
}
