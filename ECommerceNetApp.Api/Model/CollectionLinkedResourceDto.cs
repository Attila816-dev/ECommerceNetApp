namespace ECommerceNetApp.Api.Model
{
    /// <summary>
    /// Represents a collection of resources with hypermedia links.
    /// </summary>
    /// <typeparam name="T">Type of the items in the collection.</typeparam>
    public class CollectionLinkedResourceDto<T>(IEnumerable<T> items) : BaseLinkedResourceDto
    {
        /// <summary>
        /// Gets the items in this collection.
        /// </summary>
        public IEnumerable<T> Items { get; } = items;
    }
}
