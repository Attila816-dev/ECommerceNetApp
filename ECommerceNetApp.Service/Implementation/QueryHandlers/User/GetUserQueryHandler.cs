using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.User;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.User
{
    public class GetUserQueryHandler(ProductCatalogDbContext dbContext)
        : IQueryHandler<GetUserQuery, UserDto?>
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext;

        public async Task<UserDto?> HandleAsync(GetUserQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToUpper() == request.Email.ToUpper(), cancellationToken).ConfigureAwait(false);

            if (user == null)
            {
                return null;
            }

            return new UserDto(user.FirstName, user.LastName, user.Email);
        }
    }
}
