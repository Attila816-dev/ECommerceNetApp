using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.User;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.User
{
    public class GetUserQueryHandler(IProductCatalogUnitOfWork productCatalogUnitOfWork)
        : IRequestHandler<GetUserQuery, UserDto?>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork;

        public async Task<UserDto?> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var user = await _productCatalogUnitOfWork.UserRepository.GetUserByEmailAsync(request.Email, cancellationToken).ConfigureAwait(false);

            if (user == null)
            {
                return null;
            }

            return new UserDto(user.FirstName, user.LastName, user.Email);
        }
    }
}
