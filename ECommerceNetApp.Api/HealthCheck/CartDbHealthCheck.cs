using ECommerceNetApp.Persistence.Interfaces.Cart;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ECommerceNetApp.Api.HealthCheck
{
    public class CartDbHealthCheck(ICartRepository cartRepository) : IHealthCheck
    {
        private readonly ICartRepository _cartRepository = cartRepository;

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Perform a simple query to check if the database is accessible
                await _cartRepository.CountAsync(cancellationToken).ConfigureAwait(false);
                return HealthCheckResult.Healthy("CartDb is operational");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("CartDb health check failed", exception: ex);
            }
        }
    }
}
