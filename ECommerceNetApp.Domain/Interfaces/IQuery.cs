namespace ECommerceNetApp.Domain.Interfaces
{
#pragma warning disable CA1040 // Avoid empty interfaces
    /// <summary>
    /// Marker interface for defining query requests in the application.
    /// </summary>
    /// <typeparam name="TResponse">Response type.</typeparam>
    public interface IQuery<out TResponse>
    {
        // This interface intentionally left blank as a marker interface.
    }
#pragma warning restore CA1040 // Avoid empty interfaces
}
