namespace ECommerceNetApp.Domain.Options
{
    public class AWSEventBusOptions
    {
        public required string Region { get; set; }

        public required string AccessKey { get; set; }

        public required string SecretKey { get; set; }

        /// <summary>
        /// Gets or sets the Azure Service Bus topic name.
        /// </summary>
        public required string TopicName { get; set; }

        public string? DeadLetterQueueArn { get; set; }
    }
}
