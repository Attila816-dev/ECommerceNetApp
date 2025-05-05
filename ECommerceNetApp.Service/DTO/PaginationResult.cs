namespace ECommerceNetApp.Service.DTO
{
    public class PaginationResult<T>
    {
        public PaginationResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }

        public IEnumerable<T> Items { get; }

        public int TotalCount { get; }

        public int PageNumber { get; }

        public int PageSize { get; }

        public int TotalPages { get; }

        public bool HasPreviousPage => PageNumber > 1;

        public bool HasNextPage => PageNumber < TotalPages;
    }
}
