using ECommerceNetApp.Domain.Events.Product;
using ECommerceNetApp.Domain.Exceptions.Product;

namespace ECommerceNetApp.Domain.Entities
{
    public class ProductEntity : BaseEntity<int>
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
            int amount,
            bool raiseDomainEvent = true)
            : base(default)
        {
            UpdateName(name);
            UpdateDescription(description);
            UpdateImage(imageUrl);
            UpdateCategory(category);
            UpdatePrice(price);
            UpdateAmount(amount);

            if (raiseDomainEvent)
            {
                ClearDomainEvents();
                AddDomainEvent(new ProductCreatedEvent(Id, Name, Description, CategoryId, ImageUrl, Price, Amount));
            }
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
            : this(name, description, imageUrl, category, price, amount, raiseDomainEvent: false)
        {
            Id = id;

            ClearDomainEvents();
            AddDomainEvent(new ProductCreatedEvent(Id, Name, Description, CategoryId, ImageUrl, Price, Amount));
        }

        // For EF Core
        protected ProductEntity()
            : base(default)
        {
        }

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
                throw InvalidProductException.NameRequired();
            }

            if (name.Equals(Name, StringComparison.Ordinal))
            {
                return;
            }

            if (name.Length > MaxProductNameLength)
            {
                throw InvalidProductException.NameTooLong();
            }

            Name = name;
            AddDomainEvent(new ProductUpdatedEvent(Id, Name, Description, CategoryId, ImageUrl, Price, Amount));
        }

        public void UpdateDescription(string? description)
        {
            if ((string.IsNullOrEmpty(description) && string.IsNullOrEmpty(Description))
                || (!string.IsNullOrEmpty(description) && description.Equals(Description, StringComparison.Ordinal)))
            {
                return;
            }

            Description = description;
            AddDomainEvent(new ProductUpdatedEvent(Id, Name, Description, CategoryId, ImageUrl, Price, Amount));
        }

#pragma warning disable CA1054 // URI-like parameters should not be strings
        public void UpdateImage(string? imageUrl)
#pragma warning restore CA1054 // URI-like parameters should not be strings
        {
            if ((string.IsNullOrEmpty(imageUrl) && string.IsNullOrEmpty(ImageUrl)) ||
                (!string.IsNullOrEmpty(imageUrl) && !imageUrl.Equals(ImageUrl, StringComparison.Ordinal)))
            {
                return;
            }

            ImageUrl = imageUrl;
            AddDomainEvent(new ProductUpdatedEvent(Id, Name, Description, CategoryId, ImageUrl, Price, Amount));
        }

        public void UpdateCategory(CategoryEntity category)
        {
            Category = category ?? throw InvalidProductException.CategoryRequired();
            CategoryId = category.Id;
            AddDomainEvent(new ProductUpdatedEvent(Id, Name, Description, CategoryId, ImageUrl, Price, Amount));
        }

        public void UpdatePrice(decimal price)
        {
            if (price < 0)
            {
                throw InvalidProductException.InvalidPrice();
            }

            Price = price;
            AddDomainEvent(new ProductUpdatedEvent(Id, Name, Description, CategoryId, ImageUrl, Price, Amount));
        }

        public void UpdateAmount(int amount)
        {
            if (amount < 0)
            {
                throw InvalidProductException.InvalidAmount();
            }

            int oldAmount = Amount;
            Amount = amount;

            AddDomainEvent(new ProductUpdatedEvent(Id, Name, Description, CategoryId, ImageUrl, Price, Amount));

            if (oldAmount != amount)
            {
                AddDomainEvent(new ProductStockChangedEvent(Id, amount, oldAmount));
            }
        }

        public override void MarkAsDeleted()
        {
            AddDomainEvent(new ProductDeletedEvent(Id));
        }
    }
}
