using ECommerceNetApp.Domain.Entities;

namespace ECommerceNetApp.Persistence.Interfaces.ProductCatalog
{
    public interface IUserRepository : IRepository<UserEntity, int>
    {
        Task<UserEntity?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}
