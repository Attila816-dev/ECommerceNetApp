using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ECommerceNetApp.Api.Model
{
    public abstract class BaseProductDto
    {
        [Required]
        public required string Name { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        [Required]
        [JsonRequired]
        public int CategoryId { get; set; }

        [Required]
        [JsonRequired]
        [Range(0.01, 1000000)]
        public decimal Price { get; set; }

        public string? Currency { get; set; }

        [Required]
        [JsonRequired]
        [Range(1, 1000000)]
        public int Amount { get; set; }
    }
}
