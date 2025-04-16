using ECommerceNetApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog
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

            builder.Property(p => p.ImageUrl)
                   .IsRequired(false);

            builder.Property(p => p.Price)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");

            builder.Property(p => p.Amount)
                   .IsRequired();

            // Relationship with Category
            builder.HasOne(p => p.Category)
                   .WithMany(c => c.Products)
                   .HasForeignKey(p => p.CategoryId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
