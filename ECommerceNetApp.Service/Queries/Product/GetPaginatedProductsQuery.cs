using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Service.DTO;

namespace ECommerceNetApp.Service.Queries.Product
{
    public record GetPaginatedProductsQuery : IQuery<PaginationResult<ProductDto>>
    {
        public GetPaginatedProductsQuery(int pageNumber, int pageSize, int? categoryId = null)
        {
            PageNumber = pageNumber > 0 ? pageNumber : 1;
            PageSize = pageSize > 0 ? pageSize : 10;
            CategoryId = categoryId;
        }

        public int PageNumber { get; }

        public int PageSize { get; }

        public int? CategoryId { get; }
    }
}
