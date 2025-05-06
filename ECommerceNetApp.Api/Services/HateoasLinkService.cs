using ECommerceNetApp.Api.Model;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceNetApp.Api.Services
{
    /// <summary>
    /// Implementation of the HATEOAS link service.
    /// </summary>
    public class HateoasLinkService : IHateoasLinkService
    {
        public LinkDto CreateLink(ControllerBase controller, string actionName, string? controllerName = null, object? values = null, string rel = "self", string method = "GET")
        {
            ArgumentNullException.ThrowIfNull(controller);
            string url = controller.Url.Action(
                actionName,
                controllerName,
                values,
                controller.Request.Scheme)!;

            return new LinkDto(url, rel, method);
        }

        public List<LinkDto> CreatePaginationLinks(ControllerBase controller, string actionName, int pageNumber, int pageSize, int totalPages, object? additionalValues = null)
        {
            var links = new List<LinkDto>();

            // Create route values object with pagination parameters and any additional values
            var routeValues = new Dictionary<string, object>
            {
                { "pageNumber", pageNumber },
                { "pageSize", pageSize },
            };

            // Add any additional route values
            if (additionalValues != null)
            {
                foreach (var prop in additionalValues.GetType().GetProperties())
                {
                    var value = prop.GetValue(additionalValues);
                    if (value != null)
                    {
                        routeValues[prop.Name] = value;
                    }
                }
            }

            // Self link
            links.Add(CreateLink(controller, actionName, values: routeValues, rel: "self"));

            // First page
            if (pageNumber > 1)
            {
                var firstPageValues = new Dictionary<string, object>(routeValues)
                {
                    { "pageNumber", 1 },
                };
                links.Add(CreateLink(controller, actionName, values: firstPageValues, rel: "first"));
            }

            // Previous page
            if (pageNumber > 1)
            {
                var prevPageValues = new Dictionary<string, object>(routeValues)
                {
                    { "pageNumber", pageNumber - 1 },
                };
                links.Add(CreateLink(controller, actionName, values: prevPageValues, rel: "prev"));
            }

            // Next page
            if (pageNumber < totalPages)
            {
                var nextPageValues = new Dictionary<string, object>(routeValues)
                {
                    { "pageNumber", pageNumber + 1 },
                };
                links.Add(CreateLink(controller, actionName, values: nextPageValues, rel: "next"));
            }

            // Last page
            if (pageNumber < totalPages)
            {
                var lastPageValues = new Dictionary<string, object>(routeValues)
                {
                    { "pageNumber", totalPages },
                };
                links.Add(CreateLink(controller, actionName, values: lastPageValues, rel: "last"));
            }

            return links;
        }
    }
}