using ECommerceNetApp.Domain.Events.Category;
using ECommerceNetApp.Domain.Exceptions.Category;
using ECommerceNetApp.Domain.ValueObjects;

namespace ECommerceNetApp.Domain.Entities
{
    public class CategoryEntity : BaseEntity<int>
    {
        public const int MaxCategoryNameLength = 50;

        public CategoryEntity(
            string name,
            ImageInfo? image = null,
            CategoryEntity? parentCategory = null,
            bool raiseDomainEvent = true)
            : base(default)
        {
            UpdateName(name);
            UpdateImage(image);

            if (parentCategory != null)
            {
                UpdateParentCategory(parentCategory);
            }
            else
            {
                ParentCategory = null;
                ParentCategoryId = null;
            }

            if (raiseDomainEvent)
            {
                ClearDomainEvents();
                AddDomainEvent(new CategoryCreatedEvent(Id, Name, Image?.Url, ParentCategoryId));
            }
        }

        public CategoryEntity(int id, string name, ImageInfo? image = null, CategoryEntity? parentCategory = null)
            : this(name, image, parentCategory, raiseDomainEvent: false)
        {
            Id = id;

            ClearDomainEvents();
            AddDomainEvent(new CategoryCreatedEvent(Id, Name, Image?.Url, ParentCategoryId));
        }

        // For EF Core
        protected CategoryEntity()
            : base(default)
        {
        }

        public string Name { get; private set; } = string.Empty;

        public ImageInfo? Image { get; private set; }

        public int? ParentCategoryId { get; private set; }

        public virtual CategoryEntity? ParentCategory { get; private set; }

        public virtual ICollection<CategoryEntity> SubCategories { get; private set; } = new List<CategoryEntity>();

        public virtual ICollection<ProductEntity> Products { get; private set; } = new List<ProductEntity>();

        public void UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw InvalidCategoryException.NameRequired();
            }

            if (name.Equals(Name, StringComparison.Ordinal))
            {
                return;
            }

            if (name.Length > MaxCategoryNameLength)
            {
                throw InvalidCategoryException.NameTooLong();
            }

            Name = name;
            AddDomainEvent(new CategoryUpdatedEvent(Id, Name, Image?.Url, ParentCategoryId));
        }

        public void UpdateImage(ImageInfo? image)
        {
            if ((image == null && Image == null) ||
                (image != null && Image != null && image.Equals(Image)))
            {
                return;
            }

            Image = image;
            AddDomainEvent(new CategoryUpdatedEvent(Id, Name, Image?.Url, ParentCategoryId));
        }

        public void UpdateParentCategory(CategoryEntity? parentCategory)
        {
            // Check for circular reference
            if (parentCategory != null)
            {
                var parent = parentCategory;
                while (parent != null)
                {
                    if (parent.Id == this.Id)
                    {
                        throw InvalidCategoryException.CircularReference();
                    }

                    parent = parent.ParentCategory;
                }
            }

            ParentCategory = parentCategory;
            ParentCategoryId = parentCategory?.Id;
            AddDomainEvent(new CategoryUpdatedEvent(Id, Name, Image?.Url, ParentCategoryId));
        }

        public override void MarkAsDeleted()
        {
            AddDomainEvent(new CategoryDeletedEvent(Id));
        }
    }
}
