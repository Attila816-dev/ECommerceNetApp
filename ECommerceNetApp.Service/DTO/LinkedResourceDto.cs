namespace ECommerceNetApp.Service.DTO
{
    public class LinkedResourceDto<T>(T resource)
    {
        public T Resource { get; } = resource;

        public List<LinkDto> Links { get; } = new List<LinkDto>();

        public void AddLink(LinkDto link)
        {
            Links.Add(link);
        }
    }
}
