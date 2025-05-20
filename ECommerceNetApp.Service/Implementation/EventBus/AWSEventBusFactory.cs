using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.Credentials;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using ECommerceNetApp.Domain.Options;
using Microsoft.Extensions.Options;

namespace ECommerceNetApp.Service.Implementation.EventBus
{
    /// <summary>
    /// Factory for creating AWS SQS and SNS clients, useful for testability and resource management.
    /// </summary>
    public class AWSEventBusFactory : IAsyncDisposable
    {
        private readonly object _lock = new();
        private readonly AWSEventBusOptions _awsEventBusOptions;
        private AmazonSQSClient? _sqsClient;
        private AmazonSimpleNotificationServiceClient? _snsClient;

        public AWSEventBusFactory(IOptions<EventBusOptions> options)
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            _awsEventBusOptions = options.Value.AWSOptions ?? throw new InvalidOperationException("AWS Service Bus options are not configured");
        }

        public AmazonSQSClient CreateSqsClient()
        {
            if (_sqsClient == null)
            {
                lock (_lock)
                {
                    if (_sqsClient == null)
                    {
                        var credentials = GetAwsCredentials();
                        var region = GetAwsRegion();

                        _sqsClient = new AmazonSQSClient(credentials, region);
                    }
                }
            }

            return _sqsClient;
        }

        public IAmazonSimpleNotificationService CreateSnsClient()
        {
            if (_snsClient == null)
            {
                lock (_lock)
                {
                    if (_snsClient == null)
                    {
                        var credentials = GetAwsCredentials();
                        var region = GetAwsRegion();

                        _snsClient = new AmazonSimpleNotificationServiceClient(credentials, region);
                    }
                }
            }

            return _snsClient;
        }

        public async ValueTask DisposeAsync()
        {
            if (_sqsClient != null)
            {
                _sqsClient.Dispose();
                _sqsClient = null;
            }

            if (_snsClient != null)
            {
                _snsClient.Dispose();
                _snsClient = null;
            }

            await Task.CompletedTask.ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        private AWSCredentials GetAwsCredentials()
        {
            // If explicit access key and secret are provided, use them
            if (!string.IsNullOrEmpty(_awsEventBusOptions.AccessKey) && !string.IsNullOrEmpty(_awsEventBusOptions.SecretKey))
            {
                return new BasicAWSCredentials(_awsEventBusOptions.AccessKey, _awsEventBusOptions.SecretKey);
            }

            // Otherwise, use the default AWS credential provider chain
            // This will look for credentials in environment variables, the AWS SDK store, EC2 instance profiles, etc.
            return DefaultAWSCredentialsIdentityResolver.GetCredentials();
        }

        private RegionEndpoint GetAwsRegion()
        {
            // If a region is specified in options, use it
            if (!string.IsNullOrEmpty(_awsEventBusOptions.Region))
            {
                return RegionEndpoint.GetBySystemName(_awsEventBusOptions.Region);
            }

            // Default to eu-west-1 if not specified
            return RegionEndpoint.EUWest1;
        }
    }
}
