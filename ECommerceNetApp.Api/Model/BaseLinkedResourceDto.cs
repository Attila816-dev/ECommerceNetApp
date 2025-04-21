namespace ECommerceNetApp.Api.Model
{
    /// <summary>
    /// Base class for resources that include hypermedia links.
    /// </summary>
    public class BaseLinkedResourceDto
    {
        /// <summary>
        /// Gets the links related to this resource.
        /// </summary>
        public List<LinkDto> Links { get; } = new List<LinkDto>();

        /// <summary>
        /// Adds a link to this resource.
        /// </summary>
        /// <param name="link">The link to add.</param>
        public void AddLink(LinkDto link)
        {
            Links.Add(link);
        }

        /// <summary>
        /// Adds a collection of links to this resource.
        /// </summary>
        /// <param name="links">The links to add.</param>
        public void AddLinks(IEnumerable<LinkDto> links)
        {
            Links.AddRange(links);
        }
    }
}
