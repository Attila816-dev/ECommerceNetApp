using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ECommerceNetApp.Api.HealthCheck
{
    public class ProductCatalogDbHealthCheck(ProductCatalogDbContext dbContext) : IHealthCheck
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext;

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                // Perform a simple query to check if the database is accessible
                await _dbContext.Categories.AnyAsync(cancellationToken).ConfigureAwait(false);
                return HealthCheckResult.Healthy("ProductCatalogDb is operational");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("ProductCatalogDb health check failed", exception: ex);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }
    }
}
