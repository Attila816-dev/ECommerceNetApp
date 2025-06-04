using System.Text.Json.Serialization;

namespace ECommerceNetApp.Api.Model
{
    public class UpdateProductDto : BaseProductDto
    {
        [JsonRequired]
        public int Id { get; set; }
    }
}
