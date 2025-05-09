namespace ECommerceNetApp.Domain.Options
{
    public class EventBusOptions
    {
        public const string SectionName = "EventBus";

        /// <summary>
        /// Gets or sets the type of event bus to use. Valid values are "InMemory" or "Azure".
        /// </summary>
        public required string Type { get; set; }

        /// <summary>
        /// Gets or sets the Azure Service Bus connection string. Required when Type is "Azure".
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the Azure Service Bus topic name. Required when Type is "Azure".
        /// </summary>
        public string? TopicName { get; set; }

        /// <summary>
        /// Gets or sets the default time to live for messages in days.
        /// </summary>
        public int DefaultMessageTimeToLiveInDays { get; set; } = 14;

        /// <summary>
        /// Gets or sets the maximum number of concurrent calls to process messages.
        /// </summary>
        public int MaxConcurrentCalls { get; set; } = 10;

        /// <summary>
        /// Gets or sets a value indicating whether whether to auto-create topics and subscriptions if they don't exist.
        /// </summary>
        public bool AutoCreateEntities { get; set; } = true;

        /// <summary>
        /// Gets a value indicating whether to use the Azure Event Bus.
        /// </summary>
        public bool UseAzureEventBus => string.Equals(Type, "Azure", StringComparison.OrdinalIgnoreCase);
    }
}
