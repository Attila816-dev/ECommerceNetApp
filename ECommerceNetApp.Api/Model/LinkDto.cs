namespace ECommerceNetApp.Api.Model
{
    public class LinkDto(string href, string rel, string method)
    {
        public string Href { get; } = href;

        public string Rel { get; } = rel;

        public string Method { get; } = method;
    }
}
