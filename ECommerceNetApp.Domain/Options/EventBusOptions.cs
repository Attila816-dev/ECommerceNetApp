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
        /// Gets or sets the default time to live for messages in days.
        /// </summary>
        public required int DefaultMessageTimeToLiveInDays { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of concurrent calls to process messages.
        /// </summary>
        public required int MaxConcurrentCalls { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether to auto-create topics and subscriptions if they don't exist.
        /// </summary>
        public bool AutoCreateEntities { get; set; }

        public AzureEventBusOptions? AzureOptions { get; set; }

        public AWSEventBusOptions? AWSOptions { get; set; }

        /// <summary>
        /// Gets a value indicating whether to use the Azure.
        /// </summary>
        public bool UseAzure => string.Equals(Type, "Azure", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Gets a value indicating whether to use the AWS.
        /// </summary>
        public bool UseAws => string.Equals(Type, "AWS", StringComparison.OrdinalIgnoreCase);
    }
}
