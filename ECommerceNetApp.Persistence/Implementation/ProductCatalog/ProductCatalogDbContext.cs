using ECommerceNetApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog
{
    public class ProductCatalogDbContext : DbContext
    {
        public ProductCatalogDbContext(DbContextOptions<ProductCatalogDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; } = null!;

        public DbSet<Product> Products { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            base.OnModelCreating(modelBuilder);

            // Configure Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ImageUrl).IsRequired(false);

                // Self-referencing relationship for parent-child categories
                entity.HasOne(e => e.ParentCategory)
                    .WithMany(e => e.SubCategories)
                    .HasForeignKey(e => e.ParentCategoryId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).IsRequired(false);
                entity.Property(e => e.ImageUrl).IsRequired(false);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.Amount).IsRequired();

                // Relationship with Category
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(e => e.CategoryId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
