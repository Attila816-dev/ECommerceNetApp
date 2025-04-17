using MediatR;

namespace ECommerceNetApp.Domain.Events
{
    public abstract record DomainEvent : INotification
    {
        protected DomainEvent()
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
        }

        public Guid Id { get; }

        public DateTime OccurredOn { get; }
    }
}
