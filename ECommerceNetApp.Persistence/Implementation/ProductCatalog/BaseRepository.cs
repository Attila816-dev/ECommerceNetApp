using System.Linq.Expressions;
using ECommerceNetApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog
{
    internal abstract class BaseRepository<TEntity, TId>(ProductCatalogDbContext dbContext)
        where TEntity : BaseEntity<TId>
        where TId : struct
    {
        protected ProductCatalogDbContext DbContext { get; } = dbContext;

        protected DbSet<TEntity> DbSet { get; } = dbContext.Set<TEntity>();

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await DbSet.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken)
        {
            return await DbSet.FindAsync([id], cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            var property = Expression.Property(parameter, "Id");
            var constant = Expression.Constant(id);
            var equality = Expression.Equal(property, constant);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(equality, parameter);

            return await DbSet.AnyAsync(lambda, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await DbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        }

        public virtual void Update(TEntity entity)
        {
            DbSet.Update(entity);
        }

        public virtual async Task DeleteAsync(TId id, CancellationToken cancellationToken)
        {
            var entity = await DbSet.FindAsync([id], cancellationToken).ConfigureAwait(false);
            if (entity != null)
            {
                entity.MarkAsDeleted();
                DbSet.Remove(entity);
            }
        }
    }
}