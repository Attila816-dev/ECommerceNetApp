﻿using ECommerceNetApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog
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

            builder.Property(c => c.ImageUrl)
                   .IsRequired(false);

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
