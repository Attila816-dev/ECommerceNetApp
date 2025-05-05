namespace ECommerceNetApp.Api.Model
{
    /// <summary>
    /// Represents a paginated collection of resources with hypermedia links.
    /// </summary>
    /// <typeparam name="T">Type of the items in the collection.</typeparam>
    public class PagedResourceDto<T> : BaseLinkedResourceDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedResourceDto{T}"/> class.
        /// Creates a new paged collection resource.
        /// </summary>
        /// <param name="items">The items for this page.</param>
        /// <param name="currentPage">The current page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="totalCount">The total count of items.</param>
        public PagedResourceDto(IEnumerable<T> items, int currentPage, int pageSize, int totalCount)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            Pagination = new PaginationMetadata(currentPage, pageSize, totalCount);
        }

        /// <summary>
        /// Gets the items in this page.
        /// </summary>
        public IEnumerable<T> Items { get; }

        /// <summary>
        /// Gets pagination metadata.
        /// </summary>
        public PaginationMetadata Pagination { get; }
    }
}
