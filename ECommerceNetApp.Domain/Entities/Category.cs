namespace ECommerceNetApp.Domain.Entities
{
    public class Category
    {
        private const int MaxCategoryNameLength = 100;

#pragma warning disable CA1054 // URI-like parameters should not be strings
        public Category(string name, string? imageUrl = null, Category? parentCategory = null)
#pragma warning restore CA1054 // URI-like parameters should not be strings
        {
            UpdateName(name);
            ImageUrl = imageUrl;
            ParentCategory = parentCategory;
            ParentCategoryId = parentCategory?.Id;
        }

#pragma warning disable CA1054 // URI-like parameters should not be strings
        public Category(int id, string name, string? imageUrl = null, Category? parentCategory = null)
#pragma warning restore CA1054 // URI-like parameters should not be strings
            : this(name, imageUrl, parentCategory)
        {
            Id = id;
        }

        // For EF Core
        protected Category()
        {
        }

        public int Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string? ImageUrl { get; private set; }

        public int? ParentCategoryId { get; private set; }

        public virtual Category? ParentCategory { get; private set; }

        public virtual ICollection<Category> SubCategories { get; private set; } = new List<Category>();

        public virtual ICollection<Product> Products { get; private set; } = new List<Product>();

        public void UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Category name is required", nameof(name));
            }

            if (name.Length > MaxCategoryNameLength)
            {
                throw new ArgumentException("Category name cannot exceed 50 characters", nameof(name));
            }

            Name = name;
        }

#pragma warning disable CA1054 // URI-like parameters should not be strings
        public void UpdateImage(string? imageUrl)
#pragma warning restore CA1054 // URI-like parameters should not be strings
        {
            ImageUrl = imageUrl;
        }

        public void UpdateParentCategory(Category? parentCategory)
        {
            // Check for circular reference
            if (parentCategory != null)
            {
                var parent = parentCategory;
                while (parent != null)
                {
                    if (parent.Id == this.Id)
                    {
                        throw new InvalidOperationException("Circular reference detected in category hierarchy");
                    }

                    parent = parent.ParentCategory;
                }
            }

            ParentCategory = parentCategory;
            ParentCategoryId = parentCategory?.Id;
        }
    }
}
