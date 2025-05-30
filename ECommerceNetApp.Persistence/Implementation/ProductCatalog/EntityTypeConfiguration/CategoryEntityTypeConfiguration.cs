using ECommerceNetApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.EntityTypeConfiguration
{
    public class CategoryEntityTypeConfiguration : IEntityTypeConfiguration<CategoryEntity>
    {
        public void Configure(EntityTypeBuilder<CategoryEntity> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable("Categories");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(CategoryEntity.MaxCategoryNameLength);

            // Configure ImageInfo as owned entity
            builder.OwnsOne(c => c.Image, imageBuilder =>
            {
                imageBuilder.Property(i => i.Url).HasColumnName("ImageUrl");
                imageBuilder.Property(i => i.AltText).HasColumnName("ImageAltText");
            });

            builder.Ignore(c => c.DomainEvents);

            // Self-referencing relationship for parent category
            builder.HasOne(c => c.ParentCategory)
                   .WithMany(c => c.SubCategories)
                   .HasForeignKey(c => c.ParentCategoryId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
