using ECommerceNetApp.Api.Model;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceNetApp.Api.Services
{
    /// <summary>
    /// Service for generating HATEOAS links for API resources.
    /// </summary>
    public interface IHateoasLinkService
    {
        /// <summary>
        /// Generates a link for a resource.
        /// </summary>
        /// <param name="controller">The controller instance to use for URL generation.</param>
        /// <param name="actionName">Name of the controller action.</param>
        /// <param name="controllerName">Optional name of the controller if different from current.</param>
        /// <param name="values">Route values.</param>
        /// <param name="rel">Relationship of the link to the current resource.</param>
        /// <param name="method">HTTP method for the link.</param>
        /// <returns>A link representation.</returns>
        LinkDto CreateLink(ControllerBase controller, string actionName, string? controllerName = null, object? values = null, string rel = "self", string method = "GET");

        /// <summary>
        /// Creates links for paginated resources.
        /// </summary>
        /// <param name="controller">The controller instance.</param>
        /// <param name="actionName">Name of the action method.</param>
        /// <param name="pageNumber">Current page number.</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="totalPages">Total number of pages.</param>
        /// <param name="additionalValues">Additional route values.</param>
        /// <returns>Collection of links for pagination.</returns>
        List<LinkDto> CreatePaginationLinks(ControllerBase controller, string actionName, int pageNumber, int pageSize, int totalPages, object? additionalValues = null);
    }
}
