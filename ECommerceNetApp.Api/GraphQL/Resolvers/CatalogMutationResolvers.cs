using ECommerceNetApp.Api.GraphQL.InputTypes;
using ECommerceNetApp.Service.Commands.Category;
using ECommerceNetApp.Service.Commands.Product;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces;
using ECommerceNetApp.Service.Queries.Category;
using ECommerceNetApp.Service.Queries.Product;
using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Types;
using Microsoft.AspNetCore.Authorization;

namespace ECommerceNetApp.Api.GraphQL.Resolvers
{
    [ExtendObjectType(OperationType.Mutation)]
    public class CatalogMutationResolvers
    {
        /// <summary>
        /// Creates a new category.
        /// </summary>
        /// <param name="input">The category input data.</param>
        /// <param name="dispatcher">The CQRS dispatcher.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created category details.</returns>
        [Authorize(Policy = "Create:Category")]
        public async Task<CategoryDetailDto?> CreateCategoryAsync(
            CreateCategoryInput input,
            [Service] IDispatcher dispatcher,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dispatcher);
            ArgumentNullException.ThrowIfNull(input);

            var command = new CreateCategoryCommand(
                input.Name,
                input.ImageUrl,
                input.ParentCategoryId);

            var categoryId = await dispatcher.SendCommandAsync<CreateCategoryCommand, int>(command, cancellationToken).ConfigureAwait(false);

            // Fetch the created category to return complete details
            var query = new GetCategoryByIdQuery(categoryId);
            return await dispatcher.SendQueryAsync<GetCategoryByIdQuery, CategoryDetailDto?>(query, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        /// <param name="input">The category update input data.</param>
        /// <param name="dispatcher">The CQRS dispatcher.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated category details.</returns>
        [Authorize(Policy = "Update:Category")]
        public async Task<CategoryDetailDto?> UpdateCategoryAsync(
            UpdateCategoryInput input,
            [Service] IDispatcher dispatcher,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dispatcher);
            ArgumentNullException.ThrowIfNull(input);

            var command = new UpdateCategoryCommand(
                input.Id,
                input.Name,
                input.ImageUrl,
                input.ParentCategoryId);

            await dispatcher.SendCommandAsync(command, cancellationToken).ConfigureAwait(false);

            // Fetch the updated category to return complete details
            var query = new GetCategoryByIdQuery(input.Id);
            return await dispatcher.SendQueryAsync<GetCategoryByIdQuery, CategoryDetailDto?>(query, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a category and all its related products.
        /// </summary>
        /// <param name="id">The category ID to delete.</param>
        /// <param name="dispatcher">The CQRS dispatcher.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the category was deleted successfully.</returns>
        [Authorize(Policy = "Delete:Category")]
        public async Task<bool> DeleteCategoryAsync(
            int id,
            [Service] IDispatcher dispatcher,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dispatcher);

            try
            {
                var command = new DeleteCategoryCommand(id);
                await dispatcher.SendCommandAsync(command, cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="input">The product input data.</param>
        /// <param name="dispatcher">The CQRS dispatcher.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created product details.</returns>
        [Authorize(Policy = "Create:Product")]
        public async Task<ProductDto?> CreateProductAsync(
            CreateProductInput input,
            [Service] IDispatcher dispatcher,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dispatcher);
            ArgumentNullException.ThrowIfNull(input);

            var command = new CreateProductCommand(
                input.Name,
                input.Description,
                input.ImageUrl,
                input.CategoryId,
                input.Price,
                input.Currency,
                input.Amount);

            var productId = await dispatcher.SendCommandAsync<CreateProductCommand, int>(command, cancellationToken).ConfigureAwait(false);

            // Fetch the created product to return complete details
            var query = new GetProductByIdQuery(productId);
            return await dispatcher.SendQueryAsync<GetProductByIdQuery, ProductDto?>(query, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates an existing product.
        /// </summary>
        /// <param name="input">The product update input data.</param>
        /// <param name="dispatcher">The CQRS dispatcher.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated product details.</returns>
        [Authorize(Policy = "Update:Product")]
        public async Task<ProductDto?> UpdateProductAsync(
            UpdateProductInput input,
            [Service] IDispatcher dispatcher,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dispatcher);
            ArgumentNullException.ThrowIfNull(input);

            var command = new UpdateProductCommand(
                input.Id,
                input.Name,
                input.Description,
                input.ImageUrl,
                input.CategoryId,
                input.Price,
                input.Currency,
                input.Amount);

            await dispatcher.SendCommandAsync(command, cancellationToken).ConfigureAwait(false);

            // Fetch the updated product to return complete details
            var query = new GetProductByIdQuery(input.Id);
            return await dispatcher.SendQueryAsync<GetProductByIdQuery, ProductDto?>(query, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a product.
        /// </summary>
        /// <param name="id">The product ID to delete.</param>
        /// <param name="dispatcher">The CQRS dispatcher.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the product was deleted successfully.</returns>
        [Authorize(Policy = "Delete:Product")]
        public async Task<bool> DeleteProductAsync(
            int id,
            [Service] IDispatcher dispatcher,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dispatcher);

            try
            {
                var command = new DeleteProductCommand(id);
                await dispatcher.SendCommandAsync(command, cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
