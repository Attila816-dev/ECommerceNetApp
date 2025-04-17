namespace ECommerceNetApp.Service.DTO
{
    public class CategoryDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public int? ParentCategoryId { get; set; }

        public string? ParentCategoryName { get; set; }
    }
}
