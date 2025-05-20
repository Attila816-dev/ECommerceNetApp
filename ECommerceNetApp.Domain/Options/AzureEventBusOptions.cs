namespace ECommerceNetApp.Domain.Options
{
    public class AzureEventBusOptions
    {
        /// <summary>
        /// Gets or sets the Azure Service Bus connection string.
        /// </summary>
        public required string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the Azure Service Bus topic name.
        /// </summary>
        public required string TopicName { get; set; }
    }
}
