using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog
{
    internal class UserRepository(ProductCatalogDbContext dbContext)
        : BaseRepository<UserEntity, int>(dbContext), IUserRepository
    {
        public async Task<UserEntity?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await DbSet.FirstOrDefaultAsync(u => u.Email.ToUpper() == email.ToUpper(), cancellationToken).ConfigureAwait(false);
        }
    }
}
