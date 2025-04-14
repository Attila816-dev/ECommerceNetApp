namespace ECommerceNetApp.Domain
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public int? ParentCategoryId { get; set; }

        public Category? ParentCategory { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only
        public List<Category> SubCategories { get; set; } = new List<Category>();
#pragma warning restore CA2227 // Collection properties should be read only

#pragma warning disable CA2227 // Collection properties should be read only
        public List<Product> Products { get; set; } = new List<Product>();
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
