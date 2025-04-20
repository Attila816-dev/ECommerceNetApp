using ECommerceNetApp.Domain.Events;

namespace ECommerceNetApp.Domain.Entities
{
    public abstract class BaseEntity
    {
        private readonly List<DomainEvent> _domainEvents = new List<DomainEvent>();

        public IReadOnlyCollection<DomainEvent> DomainEvents
            => _domainEvents.AsReadOnly();

        public void AddDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void RemoveDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        public abstract void MarkAsDeleted();
    }

#pragma warning disable SA1402 // File may only contain a single type
    public abstract class BaseEntity<TId> : BaseEntity
#pragma warning restore SA1402 // File may only contain a single type
    {
        protected BaseEntity(TId id)
        {
            Id = id;
        }

        public TId Id { get; protected set; }
    }
}
