using System.ComponentModel.DataAnnotations;

namespace ECommerceNetApp.Api.Model
{
    public class BaseCategoryDto
    {
        [Required]
        public required string Name { get; set; }

        public string? ImageUrl { get; set; }

        public int? ParentCategoryId { get; set; }
    }
}
