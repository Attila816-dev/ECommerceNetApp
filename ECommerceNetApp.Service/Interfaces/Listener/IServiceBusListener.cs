using Azure.Messaging.ServiceBus;

namespace ECommerceNetApp.Service.Interfaces.Listener
{
    public interface IServiceBusListener
    {
        void StartListening(Func<ServiceBusReceivedMessage, Task> messageHandler, Func<ProcessErrorEventArgs, Task> errorHandler);

        Task StopListeningAsync();
    }
}