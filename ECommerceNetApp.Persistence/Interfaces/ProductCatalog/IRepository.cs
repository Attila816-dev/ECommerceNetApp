using System.Linq.Expressions;
using ECommerceNetApp.Domain.Entities;

namespace ECommerceNetApp.Persistence.Interfaces.ProductCatalog
{
    public interface IRepository<TEntity, TId>
    where TEntity : BaseEntity<TId>
    where TId : struct
    {
        Task<IEnumerable<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
            CancellationToken cancellationToken = default);

        Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPaginatedEntitiesAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
            CancellationToken cancellationToken = default);

        Task<TEntity?> GetByIdAsync(
            TId id,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);

        Task AddAsync(TEntity entity, CancellationToken cancellationToken);

        void Update(TEntity entity);

        Task DeleteAsync(TId id, CancellationToken cancellationToken);
    }
}
