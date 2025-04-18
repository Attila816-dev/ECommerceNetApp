﻿namespace ECommerceNetApp.Persistence.Interfaces.ProductCatalog
{
    public interface IProductCatalogUnitOfWork : IDisposable
    {
        IProductRepository ProductRepository { get; }

        ICategoryRepository CategoryRepository { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task CommitAsync(CancellationToken cancellationToken = default);
    }
}
