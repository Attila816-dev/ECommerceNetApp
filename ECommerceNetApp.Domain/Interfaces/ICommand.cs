namespace ECommerceNetApp.Domain.Interfaces
{
#pragma warning disable CA1040 // Avoid empty interfaces
    /// <summary>
    /// Marker interface to represent a command with a void response.
    /// </summary>
    public interface ICommand
    {
        // This interface intentionally left blank as a marker interface.
    }

    /// <summary>
    /// Marker interface to represent a command with a response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public interface ICommand<TResponse>
    {
        // This interface intentionally left blank as a marker interface.
    }
#pragma warning restore CA1040 // Avoid empty interfaces
}
