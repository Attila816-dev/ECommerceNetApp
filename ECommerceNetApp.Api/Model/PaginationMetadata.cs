namespace ECommerceNetApp.Api.Model
{
    /// <summary>
    /// Metadata for pagination.
    /// </summary>
    public class PaginationMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationMetadata"/> class.
        /// Creates new pagination metadata.
        /// </summary>
        /// <param name="currentPage">The current page number.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="totalCount">The total number of items.</param>
        public PaginationMetadata(int currentPage, int pageSize, int totalCount)
        {
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }

        /// <summary>
        /// Gets the current page number.
        /// </summary>
        public int CurrentPage { get; }

        /// <summary>
        /// Gets the number of items per page.
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// Gets the total number of items.
        /// </summary>
        public int TotalCount { get; }

        /// <summary>
        /// Gets the total number of pages.
        /// </summary>
        public int TotalPages { get; }

        /// <summary>
        /// Gets a value indicating whether there is a previous page.
        /// </summary>
        public bool HasPrevious => CurrentPage > 1;

        /// <summary>
        /// Gets a value indicating whether there is a next page.
        /// </summary>
        public bool HasNext => CurrentPage < TotalPages;
    }
}
