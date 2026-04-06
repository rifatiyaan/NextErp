using NextErp.Domain.Common;
using NextErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace NextErp.Infrastructure.Repositories
{
    public abstract class Repository<TEntity, TKey>(DbContext context)
        : IRepository<TEntity, TKey> where TKey : IComparable
        where TEntity : class, IEntity<TKey>
    {
        protected DbContext _dbContext = context;
        protected DbSet<TEntity> _dbSet = context.Set<TEntity>();

        public virtual async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual async Task RemoveAsync(TKey id)
        {
            var entityToDelete = await _dbSet.FindAsync(new object[] { id! });
            if (entityToDelete == null)
                return;
            await RemoveAsync(entityToDelete);
        }

        public virtual async Task RemoveAsync(TEntity entityToDelete)
        {
            if (entityToDelete == null)
                return;

            if (entityToDelete is ISoftDeletable soft)
            {
                soft.IsActive = false;
                await EditAsync(entityToDelete);
                return;
            }

            await Task.Run(() =>
            {
                if (_dbContext.Entry(entityToDelete).State == EntityState.Detached)
                {
                    _dbSet.Attach(entityToDelete);
                }
                _dbSet.Remove(entityToDelete);
            });
        }

        public virtual async Task RemoveAsync(Expression<Func<TEntity, bool>> filter)
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
            {
                await Task.Run(() => _dbSet.RemoveRange(_dbSet.Where(filter)));
                return;
            }

            var matches = await _dbSet.Where(filter).ToListAsync();
            await SoftDeactivateAsync(matches);
        }

        public virtual async Task EditAsync(TEntity entityToUpdate)
        {
            await Task.Run(() =>
            {
                var entry = _dbContext.Entry(entityToUpdate);
                if (entry.State == EntityState.Detached)
                {
                    _dbSet.Attach(entityToUpdate);
                    entry = _dbContext.Entry(entityToUpdate);
                }

                entry.State = EntityState.Modified;
                var keyProperties = _dbContext.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties;
                if (keyProperties == null)
                    return;

                foreach (var keyProperty in keyProperties)
                    entry.Property(keyProperty.Name).IsModified = false;
            });
        }

        public virtual async Task<TEntity> GetByIdAsync(TKey id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<int> GetCountAsync(Expression<Func<TEntity, bool>> filter = null)
        {
            IQueryable<TEntity> query = _dbSet;
            int count;

            if (filter != null)
                count = await query.CountAsync(filter);
            else
                count = await query.CountAsync();

            return count;
        }

        public virtual int GetCount(Expression<Func<TEntity, bool>> filter = null)
        {
            IQueryable<TEntity> query = _dbSet;
            int count;

            if (filter != null)
                count = query.Count(filter);
            else
                count = query.Count();

            return count;
        }

        public virtual async Task<IList<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
        {
            IQueryable<TEntity> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (include != null)
                query = include(query);

            return await query.ToListAsync();
        }

        public virtual async Task<IList<TEntity>> GetAllAsync()
        {
            IQueryable<TEntity> query = _dbSet;
            return await query.ToListAsync();
        }

        public virtual async Task<(IList<TEntity> data, int total, int totalDisplay)> GetAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int pageIndex = 1,
            int pageSize = 10,
            bool isTrackingOff = false)
        {
            IQueryable<TEntity> baseQuery = _dbSet;

            if (filter != null)
            {
                baseQuery = baseQuery.Where(filter);
            }

            var total = await baseQuery.CountAsync();
            var totalDisplay = total;

            IQueryable<TEntity> query = baseQuery;

            if (include != null)
                query = include(query);

            if (isTrackingOff)
                query = query.AsNoTracking();

            IQueryable<TEntity> orderedQuery = orderBy != null
                ? orderBy(query)
                : query;

            var data = await orderedQuery
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total, totalDisplay);
        }

        public virtual async Task<(IList<TEntity> data, int total, int totalDisplay)> GetDynamicAsync(
            Expression<Func<TEntity, bool>> filter = null,
            string orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int pageIndex = 1,
            int pageSize = 10,
            bool isTrackingOff = false) =>
            await GetDynamicFromQueryAsync(_dbSet, filter, orderBy, include, pageIndex, pageSize, isTrackingOff);

        protected async Task<(IList<TEntity> data, int total, int totalDisplay)> GetDynamicFromQueryAsync(
                    IQueryable<TEntity> rootQuery,
                    Expression<Func<TEntity, bool>> filter = null,
                    string orderBy = null,
                    Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
                    int pageIndex = 1,
                    int pageSize = 10,
                    bool isTrackingOff = false)
        {
            IQueryable<TEntity> baseQuery = rootQuery;

            if (filter != null)
                baseQuery = baseQuery.Where(filter);

            var total = await baseQuery.CountAsync();
            var totalDisplay = total;

            IQueryable<TEntity> query = baseQuery;

            if (include != null)
                query = include(query);

            if (isTrackingOff)
                query = query.AsNoTracking();

            IQueryable<TEntity> orderedQuery = orderBy != null
                ? query.OrderBy(orderBy)
                : query;

            var data = await orderedQuery
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total, totalDisplay);
        }

        public virtual async Task<IList<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool isTrackingOff = false)
        {
            IQueryable<TEntity> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (include != null)
                query = include(query);

            if (orderBy != null)
            {
                var result = orderBy(query);

                if (isTrackingOff)
                    return await result.AsNoTracking().ToListAsync();
                else
                    return await result.ToListAsync();
            }
            else
            {
                if (isTrackingOff)
                    return await query.AsNoTracking().ToListAsync();
                else
                    return await query.ToListAsync();
            }
        }

        public virtual async Task<IList<TEntity>> GetDynamicAsync(
            Expression<Func<TEntity, bool>> filter = null,
            string orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool isTrackingOff = false)
        {
            IQueryable<TEntity> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (include != null)
                query = include(query);

            if (orderBy != null)
            {
                var result = query.OrderBy(orderBy);

                if (isTrackingOff)
                    return await result.AsNoTracking().ToListAsync();
                else
                    return await result.ToListAsync();
            }
            else
            {
                if (isTrackingOff)
                    return await query.AsNoTracking().ToListAsync();
                else
                    return await query.ToListAsync();
            }
        }

        public virtual void Add(TEntity entity)
        {
            _dbSet.Add(entity);
        }

        public virtual void Remove(TKey id)
        {
            var entityToDelete = _dbSet.Find(id);
            if (entityToDelete == null)
                return;
            Remove(entityToDelete);
        }

        public virtual void Remove(TEntity entityToDelete)
        {
            if (entityToDelete == null)
                return;

            if (entityToDelete is ISoftDeletable soft)
            {
                soft.IsActive = false;
                Edit(entityToDelete);
                return;
            }

            if (_dbContext.Entry(entityToDelete).State == EntityState.Detached)
            {
                _dbSet.Attach(entityToDelete);
            }
            _dbSet.Remove(entityToDelete);
        }

        public virtual void Remove(Expression<Func<TEntity, bool>> filter)
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
            {
                _dbSet.RemoveRange(_dbSet.Where(filter));
                return;
            }

            foreach (var entity in _dbSet.Where(filter).ToList())
            {
                ((ISoftDeletable)entity).IsActive = false;
                Edit(entity);
            }
        }

        public virtual void Edit(TEntity entityToUpdate)
        {
            _dbSet.Attach(entityToUpdate);
            _dbContext.Entry(entityToUpdate).State = EntityState.Modified;
        }

        public virtual IList<TEntity> Get(Expression<Func<TEntity, bool>> filter,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
        {
            IQueryable<TEntity> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (include != null)
                query = include(query);

            return query.ToList();
        }

        public virtual IList<TEntity> GetAll()
        {
            IQueryable<TEntity> query = _dbSet;
            return query.ToList();
        }

        public virtual TEntity GetById(TKey id)
        {
            return _dbSet.Find(id);
        }

        public virtual (IList<TEntity> data, int total, int totalDisplay) Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int pageIndex = 1, int pageSize = 10, bool isTrackingOff = false)
        {
            IQueryable<TEntity> query = _dbSet;
            var total = query.Count();
            var totalDisplay = query.Count();

            if (filter != null)
            {
                query = query.Where(filter);
                totalDisplay = query.Count();
            }

            if (include != null)
                query = include(query);

            if (orderBy != null)
            {
                var result = orderBy(query).Skip((pageIndex - 1) * pageSize).Take(pageSize);
                if (isTrackingOff)
                    return (result.AsNoTracking().ToList(), total, totalDisplay);
                else
                    return (result.ToList(), total, totalDisplay);
            }
            else
            {
                var result = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
                if (isTrackingOff)
                    return (result.AsNoTracking().ToList(), total, totalDisplay);
                else
                    return (result.ToList(), total, totalDisplay);
            }
        }

        public virtual (IList<TEntity> data, int total, int totalDisplay) GetDynamic(
            Expression<Func<TEntity, bool>> filter = null,
            string orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int pageIndex = 1, int pageSize = 10, bool isTrackingOff = false)
        {
            IQueryable<TEntity> query = _dbSet;
            var total = query.Count();
            var totalDisplay = query.Count();

            if (filter != null)
            {
                query = query.Where(filter);
                totalDisplay = query.Count();
            }

            if (include != null)
                query = include(query);

            if (orderBy != null)
            {
                var result = query.OrderBy(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize);
                if (isTrackingOff)
                    return (result.AsNoTracking().ToList(), total, totalDisplay);
                else
                    return (result.ToList(), total, totalDisplay);
            }
            else
            {
                var result = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
                if (isTrackingOff)
                    return (result.AsNoTracking().ToList(), total, totalDisplay);
                else
                    return (result.ToList(), total, totalDisplay);
            }
        }

        public virtual IList<TEntity> Get(Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool isTrackingOff = false)
        {
            IQueryable<TEntity> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (include != null)
                query = include(query);

            if (orderBy != null)
            {
                var result = orderBy(query);

                if (isTrackingOff)
                    return result.AsNoTracking().ToList();
                else
                    return result.ToList();
            }
            else
            {
                if (isTrackingOff)
                    return query.AsNoTracking().ToList();
                else
                    return query.ToList();
            }
        }

        public virtual IList<TEntity> GetDynamic(Expression<Func<TEntity, bool>> filter = null,
            string orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool isTrackingOff = false)
        {
            IQueryable<TEntity> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (include != null)
                query = include(query);

            if (orderBy != null)
            {
                var result = query.OrderBy(orderBy);

                if (isTrackingOff)
                    return result.AsNoTracking().ToList();
                else
                    return result.ToList();
            }
            else
            {
                if (isTrackingOff)
                    return query.AsNoTracking().ToList();
                else
                    return query.ToList();
            }
        }

        public async Task<IEnumerable<TResult>> GetAsync<TResult>(Expression<Func<TEntity, TResult>>? selector,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
            bool disableTracking = true,
            CancellationToken cancellationToken = default) where TResult : class
        {
            var query = _dbSet.AsQueryable();
            if (disableTracking) query.AsNoTracking();
            if (include is not null) query = include(query);
            if (predicate is not null) query = query.Where(predicate);
            return orderBy is not null
                ? await orderBy(query).Select(selector!).ToListAsync(cancellationToken)
                : await query.Select(selector!).ToListAsync(cancellationToken);
        }

        public async Task<TResult> SingleOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult>>? selector,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
            bool disableTracking = true)
        {
            var query = _dbSet.AsQueryable();
            if (disableTracking) query.AsNoTracking();
            if (include is not null) query = include(query);
            if (predicate is not null) query = query.Where(predicate);
            return (orderBy is not null
                ? await orderBy(query).Select(selector!).FirstOrDefaultAsync()
                : await query.Select(selector!).FirstOrDefaultAsync())!;
        }

        public Task<(IList<TEntity>, int total, int totalDisplay)> GetDynamicAsync(IQueryable<TEntity> query, string? orderBy, object? includes, int pageIndex, int pageSize)
        {
            throw new NotImplementedException();
        }

        private async Task SoftDeactivateAsync(List<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                ((ISoftDeletable)entity).IsActive = false;
                await EditAsync(entity);
            }
        }
    }
}
