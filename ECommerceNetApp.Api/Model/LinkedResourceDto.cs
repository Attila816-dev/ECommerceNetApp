namespace ECommerceNetApp.Api.Model
{
    /// <summary>
    /// Represents a resource with embedded data and hypermedia links.
    /// </summary>
    /// <typeparam name="T">Type of the data contained in this resource.</typeparam>
    public class LinkedResourceDto<T>(T resource) : BaseLinkedResourceDto
    {
        public T Resource { get; } = resource;
    }
}
