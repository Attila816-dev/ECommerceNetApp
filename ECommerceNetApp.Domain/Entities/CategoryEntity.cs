using ECommerceNetApp.Domain.Events.Category;
using ECommerceNetApp.Domain.Exceptions.Category;

namespace ECommerceNetApp.Domain.Entities
{
    public class CategoryEntity : BaseEntity
    {
        public const int MaxCategoryNameLength = 50;

#pragma warning disable CA1054 // URI-like parameters should not be strings
        public CategoryEntity(string name, string? imageUrl = null, CategoryEntity? parentCategory = null, bool raiseDomainEvent = true)
#pragma warning restore CA1054 // URI-like parameters should not be strings
        {
            UpdateName(name);
            UpdateImage(imageUrl);

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
                AddDomainEvent(new CategoryCreatedEvent(Id, Name, ImageUrl, ParentCategoryId));
            }
        }

#pragma warning disable CA1054 // URI-like parameters should not be strings
        public CategoryEntity(int id, string name, string? imageUrl = null, CategoryEntity? parentCategory = null)
#pragma warning restore CA1054 // URI-like parameters should not be strings
            : this(name, imageUrl, parentCategory, raiseDomainEvent: false)
        {
            Id = id;

            ClearDomainEvents();
            AddDomainEvent(new CategoryCreatedEvent(Id, Name, ImageUrl, ParentCategoryId));
        }

        // For EF Core
        protected CategoryEntity()
        {
        }

        public int Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string? ImageUrl { get; private set; }

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

            if (name.Length > MaxCategoryNameLength)
            {
                throw InvalidCategoryException.NameTooLong();
            }

            Name = name;
            AddDomainEvent(new CategoryUpdatedEvent(Id, Name, ImageUrl, ParentCategoryId));
        }

#pragma warning disable CA1054 // URI-like parameters should not be strings
        public void UpdateImage(string? imageUrl)
#pragma warning restore CA1054 // URI-like parameters should not be strings
        {
            ImageUrl = imageUrl;
            AddDomainEvent(new CategoryUpdatedEvent(Id, Name, ImageUrl, ParentCategoryId));
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
            AddDomainEvent(new CategoryUpdatedEvent(Id, Name, ImageUrl, ParentCategoryId));
        }

        public void MarkAsDeleted()
        {
            AddDomainEvent(new CategoryDeletedEvent(Id));
        }
    }
}
