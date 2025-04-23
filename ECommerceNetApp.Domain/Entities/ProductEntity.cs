using ECommerceNetApp.Domain.Events.Product;
using ECommerceNetApp.Domain.Exceptions.Product;
using ECommerceNetApp.Domain.ValueObjects;

namespace ECommerceNetApp.Domain.Entities
{
    public class ProductEntity : BaseEntity<int>
    {
        public const int MaxProductNameLength = 50;

        public ProductEntity(
            string name,
            string? description,
            ImageInfo? image,
            CategoryEntity category,
            Money price,
            int amount,
            bool raiseDomainEvent = true)
            : base(default)
        {
            UpdateName(name);
            UpdateDescription(description);
            UpdateImage(image);
            UpdateCategory(category);
            UpdatePrice(price);
            UpdateAmount(amount);

            if (raiseDomainEvent)
            {
                ClearDomainEvents();
                AddDomainEvent(new ProductCreatedEvent(Id, Name, Description, CategoryId, Image, Price, Amount));
            }
        }

        public ProductEntity(
            int id,
            string name,
            string? description,
            ImageInfo? image,
            CategoryEntity category,
            Money price,
            int amount)
            : this(name, description, image, category, price, amount, raiseDomainEvent: false)
        {
            Id = id;

            ClearDomainEvents();
            AddDomainEvent(new ProductCreatedEvent(Id, Name, Description, CategoryId, Image, Price, Amount));
        }

        // For EF Core
        protected ProductEntity()
            : base(default)
        {
        }

        public string Name { get; private set; } = string.Empty;

        public string? Description { get; private set; }

        public ImageInfo? Image { get; private set; }

        public int CategoryId { get; private set; }

        public virtual CategoryEntity? Category { get; private set; }

        public Money Price { get; private set; } = Money.From(0);

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
            AddDomainEvent(new ProductUpdatedEvent(Id, Name, Description, CategoryId, Image, Price, Amount));
        }

        public void UpdateDescription(string? description)
        {
            if ((string.IsNullOrEmpty(description) && string.IsNullOrEmpty(Description))
                || (!string.IsNullOrEmpty(description) && description.Equals(Description, StringComparison.Ordinal)))
            {
                return;
            }

            Description = description;
            AddDomainEvent(new ProductUpdatedEvent(Id, Name, Description, CategoryId, Image, Price, Amount));
        }

        public void UpdateImage(ImageInfo? image)
        {
            if ((image == null && Image == null) ||
                (image != null && Image != null && image.Equals(Image)))
            {
                return;
            }

            Image = image;
            AddDomainEvent(new ProductUpdatedEvent(Id, Name, Description, CategoryId, Image, Price, Amount));
        }

        public void UpdateCategory(CategoryEntity category)
        {
            Category = category ?? throw InvalidProductException.CategoryRequired();
            CategoryId = category.Id;
            AddDomainEvent(new ProductUpdatedEvent(Id, Name, Description, CategoryId, Image, Price, Amount));
        }

        public void UpdatePrice(Money price)
        {
            ArgumentNullException.ThrowIfNull(price);

            if (price.Equals(Price))
            {
                return;
            }

            Price = price;
            AddDomainEvent(new ProductUpdatedEvent(Id, Name, Description, CategoryId, Image, Price, Amount));
        }

        public void UpdateAmount(int amount)
        {
            if (amount < 0)
            {
                throw InvalidProductException.InvalidAmount();
            }

            int oldAmount = Amount;
            Amount = amount;

            AddDomainEvent(new ProductUpdatedEvent(Id, Name, Description, CategoryId, Image, Price, Amount));

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
