using ECommerceNetApp.Domain.Events.Product;
using ECommerceNetApp.Domain.Exceptions.Product;
using ECommerceNetApp.Domain.ValueObjects;

namespace ECommerceNetApp.Domain.Entities
{
    public class ProductEntity : BaseEntity<int>
    {
        public const int MaxProductNameLength = 50;

        internal ProductEntity(
            int? id,
            string name,
            string? description,
            ImageInfo? image,
            CategoryEntity category,
            Money price,
            int amount)
            : base(default)
        {
            if (id.HasValue)
            {
                Id = id.Value;
            }

            UpdateName(name);
            UpdateDescription(description);
            UpdateImage(image);
            UpdateCategory(category);
            UpdatePrice(price);
            UpdateAmount(amount);
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

        public static ProductEntity Create(
            string name,
            string? description,
            ImageInfo? image,
            CategoryEntity category,
            Money price,
            int amount,
            int? id = null)
        {
            var product = new ProductEntity(id, name, description, image, category, price, amount);
            product.AddDomainEvent(new ProductCreatedEvent(
                product.Name,
                product.Description,
                product.CategoryId,
                product.Image,
                product.Price,
                product.Amount));

            return product;
        }

        public void Update(
            string name,
            string? description,
            ImageInfo? image,
            CategoryEntity category,
            Money price,
            int amount)
        {
            UpdateName(name);
            UpdateDescription(description);
            UpdateImage(image);
            UpdateCategory(category);
            UpdatePrice(price);
            UpdateAmount(amount);
            AddDomainEvent(new ProductUpdatedEvent(Id, Name, Description, CategoryId, Image, Price, Amount));
        }

        public override void MarkAsDeleted()
        {
            AddDomainEvent(new ProductDeletedEvent(Id));
        }

        internal void UpdateName(string name)
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
        }

        internal void UpdateDescription(string? description)
        {
            if ((string.IsNullOrEmpty(description) && string.IsNullOrEmpty(Description))
                || (!string.IsNullOrEmpty(description) && description.Equals(Description, StringComparison.Ordinal)))
            {
                return;
            }

            Description = description;
        }

        internal void UpdateImage(ImageInfo? image)
        {
            if ((image == null && Image == null) ||
                (image != null && Image != null && image.Equals(Image)))
            {
                return;
            }

            Image = image;
        }

        internal void UpdateCategory(CategoryEntity category)
        {
            ArgumentNullException.ThrowIfNull(category);
            if (CategoryId == category.Id)
            {
                return;
            }

            Category = category;
            CategoryId = category.Id;
        }

        internal void UpdatePrice(Money price)
        {
            ArgumentNullException.ThrowIfNull(price);

            if (price.Equals(Price))
            {
                return;
            }

            Price = price;
        }

        internal void UpdateAmount(int amount)
        {
            if (amount < 0)
            {
                throw InvalidProductException.InvalidAmount();
            }

            int oldAmount = Amount;
            Amount = amount;

            if (oldAmount != amount)
            {
                AddDomainEvent(new ProductStockChangedEvent(Id, amount, oldAmount));
            }
        }
    }
}
