namespace ECommerceNetApp.Service.DTO
{
    public class CategoryDetailDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public int? ParentCategoryId { get; set; }

        public string? ParentCategoryName { get; set; }

        public IEnumerable<CategoryDto>? Subcategories { get; set; }

        public IEnumerable<ProductDto>? Products { get; set; }
    }
}
