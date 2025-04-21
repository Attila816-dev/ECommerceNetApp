using System.ComponentModel.DataAnnotations;

namespace ECommerceNetApp.Api.Model
{
    public abstract class BaseProductDto
    {
        [Required]
        public required string Name { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [Range(0.01, 1000000)]
        public decimal Price { get; set; }

        [Required]
        [Range(1, 1000000)]
        public int Amount { get; set; }
    }
}
