using ECommerceNetApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.EntityTypeConfiguration
{
    public class ProductEntityTypeConfiguration : IEntityTypeConfiguration<ProductEntity>
    {
        public void Configure(EntityTypeBuilder<ProductEntity> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable("Products");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                   .IsRequired()
                   .HasMaxLength(ProductEntity.MaxProductNameLength);

            builder.Property(p => p.Description)
                   .IsRequired(false);

            // Configure ImageInfo as owned entity
            builder.OwnsOne(p => p.Image, imageBuilder =>
            {
                imageBuilder.Property(i => i.Url).HasColumnName("ImageUrl");
                imageBuilder.Property(i => i.AltText).HasColumnName("ImageAltText");
            });

            // Configure Money as owned entity
            builder.OwnsOne(p => p.Price, priceBuilder =>
            {
                priceBuilder.Property(m => m.Amount)
                    .HasColumnName("Price")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                priceBuilder.Property(m => m.Currency)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            builder.Property(p => p.Amount)
                   .IsRequired();

            builder.Ignore(p => p.DomainEvents);

            // Relationship with Category
            builder.HasOne(p => p.Category)
                   .WithMany(c => c.Products)
                   .HasForeignKey(p => p.CategoryId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}