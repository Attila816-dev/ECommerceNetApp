using ECommerceNetApp.Domain.Entities;

namespace ECommerceNetApp.Domain.Interfaces
{
    public interface IDomainEventService
    {
        Task PublishEventsAsync(BaseEntity entity, CancellationToken cancellationToken);
    }
}
