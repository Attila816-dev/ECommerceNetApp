using ECommerceNetApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog
{
    public class UserEntityTypeConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable("Users");
            builder.HasKey(p => p.Id);

            builder.Property(e => e.Email).IsRequired().HasMaxLength(UserEntity.MaxEmailLength);
            builder.Property(e => e.PasswordHash).IsRequired();
            builder.Property(e => e.FirstName).HasMaxLength(UserEntity.MaxFirstNameLength);
            builder.Property(e => e.LastName).HasMaxLength(UserEntity.MaxLastNameLength);

            builder.HasIndex(e => e.Email).IsUnique();

            builder.Ignore(p => p.DomainEvents);
            builder.Ignore(p => p.FullName);
        }
    }
}