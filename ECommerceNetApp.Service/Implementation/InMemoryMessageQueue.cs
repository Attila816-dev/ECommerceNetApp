using System.Threading.Channels;
using ECommerceNetApp.Domain.Interfaces;

namespace ECommerceNetApp.Service.Implementation
{
    internal sealed class InMemoryMessageQueue
    {
        private readonly Channel<INotification> _channel =
            Channel.CreateUnbounded<INotification>();

        public ChannelReader<INotification> Reader => _channel.Reader;

        public ChannelWriter<INotification> Writer => _channel.Writer;
    }
}
