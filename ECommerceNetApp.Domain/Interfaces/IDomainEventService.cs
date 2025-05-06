using ECommerceNetApp.Domain.Events;

namespace ECommerceNetApp.Domain.Interfaces
{
    public interface IDomainEventService
    {
        Task PublishEventAsync(DomainEvent domainEvent, CancellationToken cancellationToken);
    }
}
