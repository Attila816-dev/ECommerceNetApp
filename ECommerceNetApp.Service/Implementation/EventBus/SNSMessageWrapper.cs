namespace ECommerceNetApp.Service.Implementation.EventBus
{
    // Class to deserialize SNS messages that come through SQS
    internal sealed class SNSMessageWrapper
    {
        public string? Message { get; set; }

        public string? MessageId { get; set; }

        public string? Subject { get; set; }

        public string? TopicArn { get; set; }

        public string? Type { get; set; }
    }
}
