using ECommerceNetApp.Api.Model;
using ECommerceNetApp.Api.Services;
using ECommerceNetApp.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceNetApp.Api.Controllers
{
    /// <summary>
    /// Base controller class for API endpoints.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class BaseApiController(IHateoasLinkService linkService, IDispatcher dispatcher) : ControllerBase
    {
        /// <summary>
        /// Gets the service for creating HATEOAS links.
        /// </summary>
        protected IHateoasLinkService LinkService { get; } = linkService;

        /// <summary>
        /// Gets the dispatcher for handling queries and commands.
        /// </summary>
        protected IDispatcher Dispatcher { get; } = dispatcher;

        /// <summary>
        /// Creates a standard resource response with HATEOAS links.
        /// </summary>
        /// <typeparam name="T">Type of the resource data.</typeparam>
        /// <param name="data">The resource data.</param>
        /// <param name="links">Links to add to the resource.</param>
        /// <returns>A resource with links.</returns>
        protected LinkedResourceDto<T> CreateResource<T>(T data, IEnumerable<LinkDto>? links = null)
        {
            var resource = new LinkedResourceDto<T>(data);

            if (links != null)
            {
                resource.AddLinks(links);
            }

            return resource;
        }

        /// <summary>
        /// Creates a standard collection resource response with HATEOAS links.
        /// </summary>
        /// <typeparam name="T">Type of the collection items.</typeparam>
        /// <param name="items">The collection items.</param>
        /// <param name="links">Links to add to the collection.</param>
        /// <returns>A collection resource with links.</returns>
        protected CollectionLinkedResourceDto<T> CreateCollectionResource<T>(IEnumerable<T> items, IEnumerable<LinkDto>? links = null)
        {
            var resource = new CollectionLinkedResourceDto<T>(items);

            if (links != null)
            {
                resource.AddLinks(links);
            }

            return resource;
        }

        /// <summary>
        /// Creates a standard paged resource response with HATEOAS links.
        /// </summary>
        /// <typeparam name="T">Type of the page items.</typeparam>
        /// <param name="items">The page items.</param>
        /// <param name="pageNumber">Current page number.</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="totalCount">Total count of items.</param>
        /// <param name="links">Links to add to the page.</param>
        /// <returns>A paged resource with links.</returns>
        protected PagedResourceDto<T> CreatePagedResource<T>(
            IEnumerable<T> items,
            int pageNumber,
            int pageSize,
            int totalCount,
            IEnumerable<LinkDto>? links = null)
        {
            var resource = new PagedResourceDto<T>(items, pageNumber, pageSize, totalCount);

            if (links != null)
            {
                resource.AddLinks(links);
            }

            return resource;
        }

        /// <summary>
        /// Creates a collection of links for a specific resource.
        /// </summary>
        /// <param name="id">Resource ID.</param>
        /// <param name="getSelfAction">Action name for getting the resource.</param>
        /// <param name="updateAction">Action name for updating the resource.</param>
        /// <param name="deleteAction">Action name for deleting the resource.</param>
        /// <returns>Collection of standard resource links.</returns>
        protected IEnumerable<LinkDto> CreateResourceLinks(
            object id,
            string getSelfAction,
            string updateAction,
            string deleteAction)
        {
            var links = new List<LinkDto>
            {
                LinkService.CreateLink(this, getSelfAction, values: new { id }, rel: "self"),
                LinkService.CreateLink(this, updateAction, values: new { id }, rel: "update", method: "PUT"),
                LinkService.CreateLink(this, deleteAction, values: new { id }, rel: "delete", method: "DELETE"),
            };

            return links;
        }
    }
}
