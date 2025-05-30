using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerceNetApp.Service.UnitTest.Extensions
{
    public static class MockProductCatalogDbContextFactory
    {
        public static (Mock<ProductCatalogDbContext> DbContext, IEventBus EventBus) Create()
        {
            var mockEventBus = new Mock<IEventBus>();
            var mockLogger = new Mock<ILogger<ProductCatalogDbContext>>();
            var mockDbContext = new Mock<ProductCatalogDbContext>(new DbContextOptions<ProductCatalogDbContext>(), mockEventBus.Object, mockLogger.Object);
            return (mockDbContext, mockEventBus.Object);
        }
    }
}
