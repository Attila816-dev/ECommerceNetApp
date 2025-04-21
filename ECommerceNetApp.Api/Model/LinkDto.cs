namespace ECommerceNetApp.Api.Model
{
    /// <summary>
    /// Represents a hypermedia link for HATEOAS.
    /// </summary>
    public class LinkDto(string href, string rel, string method)
    {
        /// <summary>
        /// Gets the URL of the link.
        /// </summary>
        public string Href { get; } = href;

        /// <summary>
        /// Gets the relationship of the link to the current resource.
        /// </summary>
        public string Rel { get; } = rel;

        /// <summary>
        /// Gets the HTTP method to use with this link.
        /// </summary>
        public string Method { get; } = method;
    }
}
