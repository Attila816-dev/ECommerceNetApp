namespace ECommerceNetApp.Domain.Entities
{
    public class ProductEntity : BaseEntity
    {
        public const int MaxProductNameLength = 50;

        public ProductEntity(
            string name,
            string? description,
#pragma warning disable CA1054 // URI-like parameters should not be strings
            string? imageUrl,
#pragma warning restore CA1054 // URI-like parameters should not be strings
            CategoryEntity category,
            decimal price,
            int amount)
        {
            UpdateName(name);
            Description = description;
            UpdateImage(imageUrl);
            UpdateCategory(category);
            UpdatePrice(price);
            UpdateAmount(amount);
        }

        public ProductEntity(
            int id,
            string name,
            string? description,
#pragma warning disable CA1054 // URI-like parameters should not be strings
            string? imageUrl,
#pragma warning restore CA1054 // URI-like parameters should not be strings
            CategoryEntity category,
            decimal price,
            int amount)
            : this(name, description, imageUrl, category, price, amount)
        {
            Id = id;
        }

        // For EF Core
        protected ProductEntity()
        {
        }

        public int Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string? Description { get; private set; }

        public string? ImageUrl { get; private set; }

        public int CategoryId { get; private set; }

        public virtual CategoryEntity? Category { get; private set; }

        public decimal Price { get; private set; }

        public int Amount { get; private set; }

        public void UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Product name is required", nameof(name));
            }

            if (name.Length > MaxProductNameLength)
            {
                throw new ArgumentException($"Product name cannot exceed {MaxProductNameLength} characters", nameof(name));
            }

            Name = name;
        }

        public void UpdateDescription(string? description)
        {
            Description = description;
        }

#pragma warning disable CA1054 // URI-like parameters should not be strings
        public void UpdateImage(string? imageUrl)
#pragma warning restore CA1054 // URI-like parameters should not be strings
        {
            ImageUrl = imageUrl;
        }

        public void UpdateCategory(CategoryEntity category)
        {
            Category = category ?? throw new ArgumentNullException(nameof(category), "Category is required");
            CategoryId = category.Id;
        }

        public void UpdatePrice(decimal price)
        {
            if (price < 0)
            {
                throw new ArgumentException("Price cannot be negative", nameof(price));
            }

            Price = price;
        }

        public void UpdateAmount(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentException("Amount must be a positive integer", nameof(amount));
            }

            Amount = amount;
        }
    }
}
