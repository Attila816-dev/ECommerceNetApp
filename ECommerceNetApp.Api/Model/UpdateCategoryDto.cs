using System.Text.Json.Serialization;

namespace ECommerceNetApp.Api.Model
{
    public class UpdateCategoryDto : BaseCategoryDto
    {
        [JsonRequired]
        public int Id { get; set; }
    }
}
