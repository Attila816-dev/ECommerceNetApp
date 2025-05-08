namespace ECommerceNetApp.Domain.Options
{
    public class EventBusOptions
    {
        public const string SectionName = "EventBus";

        /// <summary>
        /// Gets or sets the type of event bus to use. Valid values are "InMemory" or "Azure".
        /// </summary>
        public string Type { get; set; } = "InMemory";

        /// <summary>
        /// Gets or sets the Azure Service Bus connection string. Required when Type is "Azure".
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the Azure Service Bus topic name. Required when Type is "Azure".
        /// </summary>
        public string TopicName { get; set; } = "ecommerce-events";

        public int DefaultMessageTimeToLiveInDays { get; set; } = 14;
    }
}
