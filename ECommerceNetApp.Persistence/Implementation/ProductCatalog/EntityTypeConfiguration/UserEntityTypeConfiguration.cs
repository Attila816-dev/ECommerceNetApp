using ECommerceNetApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.EntityTypeConfiguration
{
    public class UserEntityTypeConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable("Users");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.PasswordHash)
                   .IsRequired()
                   .HasMaxLength(UserEntity.MaxPasswordHashLength);

            builder.Property(p => p.Email)
                   .IsRequired()
                   .HasMaxLength(UserEntity.MaxEmailLength);

            builder.Property(p => p.FirstName)
                   .IsRequired()
                   .HasMaxLength(UserEntity.MaxFirstNameLength);

            builder.Property(p => p.LastName)
                   .IsRequired()
                   .HasMaxLength(UserEntity.MaxLastNameLength);
        }
    }
}