using ECommerceNetApp.Domain.Events.Category;
using ECommerceNetApp.Domain.Exceptions.Category;
using ECommerceNetApp.Domain.ValueObjects;

namespace ECommerceNetApp.Domain.Entities
{
    public class CategoryEntity : BaseEntity<int>
    {
        public const int MaxCategoryNameLength = 50;

        internal CategoryEntity(
            int? id,
            string name,
            ImageInfo? image = null,
            CategoryEntity? parentCategory = null)
            : base(default)
        {
            if (id.HasValue)
            {
                Id = id.Value;
            }

            UpdateName(name);
            UpdateImage(image);
            UpdateParentCategory(parentCategory);
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

        public static CategoryEntity Create(
            string name,
            ImageInfo? image,
            CategoryEntity? parentCategory,
            int? id = null)
        {
            var category = new CategoryEntity(id, name, image, parentCategory);
            category.AddDomainEvent(new CategoryCreatedEvent(category.Name, category.Image, category.ParentCategoryId));
            return category;
        }

        public void Update(
            string name,
            ImageInfo? image,
            CategoryEntity? parentCategory)
        {
            UpdateName(name);
            UpdateImage(image);
            UpdateParentCategory(parentCategory);
            AddDomainEvent(new CategoryUpdatedEvent(Id, Name, Image, ParentCategoryId));
        }

        public override void MarkAsDeleted()
        {
            AddDomainEvent(new CategoryDeletedEvent(Id));
        }

        internal void UpdateName(string name)
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

        internal void UpdateParentCategory(CategoryEntity? parentCategory)
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

                ParentCategory = parentCategory;
                ParentCategoryId = parentCategory!.Id;
            }
            else
            {
                ParentCategory = null;
                ParentCategoryId = null;
            }
        }
    }
}
