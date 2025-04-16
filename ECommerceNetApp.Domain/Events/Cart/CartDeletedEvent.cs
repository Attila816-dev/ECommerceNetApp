namespace ECommerceNetApp.Domain.Events.Cart
{
    public record CartDeletedEvent(string CartId) : DomainEvent;
}
