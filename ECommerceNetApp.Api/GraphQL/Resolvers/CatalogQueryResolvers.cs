using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces;
using ECommerceNetApp.Service.Queries.Category;
using ECommerceNetApp.Service.Queries.Product;
using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Types;

namespace ECommerceNetApp.Api.GraphQL.Resolvers
{
    [ExtendObjectType(OperationType.Query)]
    public class CatalogQueryResolvers
    {
        /// <summary>
        /// Gets all categories.
        /// </summary>
        /// <param name="dispatcher">The CQRS dispatcher.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of categories.</returns>
        public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync(
            [Service] IDispatcher dispatcher,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dispatcher);
            var query = new GetAllCategoriesQuery();
            return await dispatcher.SendQueryAsync<GetAllCategoriesQuery, IEnumerable<CategoryDto>>(query, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets categories by parent category ID.
        /// </summary>
        /// <param name="parentCategoryId">The parent category ID.</param>
        /// <param name="dispatcher">The CQRS dispatcher.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of categories.</returns>
        public async Task<IEnumerable<CategoryDto>> GetCategoriesByParentAsync(
            int? parentCategoryId,
            [Service] IDispatcher dispatcher,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dispatcher);
            var query = new GetCategoriesByParentCategoryIdQuery(parentCategoryId);
            return await dispatcher.SendQueryAsync<GetCategoriesByParentCategoryIdQuery, IEnumerable<CategoryDto>>(query, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a category by ID.
        /// </summary>
        /// <param name="id">The category ID.</param>
        /// <param name="dispatcher">The CQRS dispatcher.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Category details or null if not found.</returns>
        public async Task<CategoryDetailDto?> GetCategoryByIdAsync(
            int id,
            [Service] IDispatcher dispatcher,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dispatcher);
            var query = new GetCategoryByIdQuery(id);
            return await dispatcher.SendQueryAsync<GetCategoryByIdQuery, CategoryDetailDto?>(query, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets all products.
        /// </summary>
        /// <param name="dispatcher">The CQRS dispatcher.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of products.</returns>
        public async Task<IEnumerable<ProductDto>> GetProductsAsync(
            [Service] IDispatcher dispatcher,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dispatcher);
            var query = new GetAllProductsQuery();
            return await dispatcher.SendQueryAsync<GetAllProductsQuery, IEnumerable<ProductDto>>(query, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets products by category ID.
        /// </summary>
        /// <param name="categoryId">The category ID.</param>
        /// <param name="dispatcher">The CQRS dispatcher.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of products in the category.</returns>
        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(
            int categoryId,
            [Service] IDispatcher dispatcher,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dispatcher);
            var query = new GetProductsByCategoryQuery(categoryId);
            return await dispatcher.SendQueryAsync<GetProductsByCategoryQuery, IEnumerable<ProductDto>>(query, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a product by ID.
        /// </summary>
        /// <param name="id">The product ID.</param>
        /// <param name="dispatcher">The CQRS dispatcher.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Product details or null if not found.</returns>
        public async Task<ProductDto?> GetProductByIdAsync(
            int id,
            [Service] IDispatcher dispatcher,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dispatcher);
            var query = new GetProductByIdQuery(id);
            return await dispatcher.SendQueryAsync<GetProductByIdQuery, ProductDto?>(query, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets paginated products with optional category filtering.
        /// </summary>
        /// <param name="pageNumber">The page number (default: 1).</param>
        /// <param name="pageSize">The page size (default: 10).</param>
        /// <param name="categoryId">Optional category ID filter.</param>
        /// <param name="dispatcher">The CQRS dispatcher.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Paginated products result.</returns>
        public async Task<PaginationResult<ProductDto>> GetPaginatedProductsAsync(
            int pageNumber = 1,
            int pageSize = 10,
            int? categoryId = null,
            [Service] IDispatcher? dispatcher = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dispatcher);
            var query = new GetPaginatedProductsQuery(pageNumber, pageSize, categoryId);
            return await dispatcher.SendQueryAsync<GetPaginatedProductsQuery, PaginationResult<ProductDto>>(query, cancellationToken).ConfigureAwait(false);
        }
    }
}
