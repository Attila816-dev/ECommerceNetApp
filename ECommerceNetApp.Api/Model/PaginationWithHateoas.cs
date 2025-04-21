using ECommerceNetApp.Service.DTO;

namespace ECommerceNetApp.Api.Model
{
    public class PaginationWithHateoas<T>(
            int pageSize,
            int pageNumber,
            int totalCount,
            int totalPages,
            bool hasNextPage,
            bool hasPreviousPage,
            IEnumerable<T> items,
            IEnumerable<LinkDto> links)
    {
        public int PageSize { get; } = pageSize;

        public int PageNumber { get; } = pageNumber;

        public int TotalCount { get; } = totalCount;

        public int TotalPages { get; } = totalPages;

        public bool HasNextPage { get; } = hasNextPage;

        public bool HasPreviousPage { get; } = hasPreviousPage;

        public IEnumerable<T> Items { get; } = items;

        public IEnumerable<LinkDto> Links { get; } = links;

#pragma warning disable CA1000 // Do not declare static members on generic types
        public static PaginationWithHateoas<T> FromPaginationResult(
            PaginationResult<T> paginationResult,
            IEnumerable<LinkDto> links)
        {
            ArgumentNullException.ThrowIfNull(paginationResult, nameof(paginationResult));
            ArgumentNullException.ThrowIfNull(links, nameof(links));

            return new PaginationWithHateoas<T>(
                paginationResult.PageSize,
                paginationResult.PageNumber,
                paginationResult.TotalCount,
                paginationResult.TotalPages,
                paginationResult.HasNextPage,
                paginationResult.HasPreviousPage,
                paginationResult.Items,
                links);
        }
#pragma warning restore CA1000 // Do not declare static members on generic types
    }
}
