namespace ECommerceNetApp.Domain.Events
{
    public abstract class DomainEvent
    {
        protected DomainEvent()
        {
            OccurredOn = DateTime.UtcNow;
        }

        public DateTime OccurredOn { get; }
    }
}
