using System.Linq.Expressions;
using Learning_Platform_app.Common.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Learning_Platform_app.Infrastructure.Persistence.Repositories;

public sealed class GenericRepository<T>(ApplicationDbContext context) : IGenericRepository<T>
    where T : class
{
    private readonly DbSet<T> entities = context.Set<T>();

    public IQueryable<T> GetAll(Expression<Func<T, bool>>? filter = null,bool tracking = false)
    {
        IQueryable<T> query = tracking
            ? entities.AsTracking() : entities.AsNoTracking();

        if (filter is not null)
        {
            query = query.Where(filter);
        }

        return query;
    }
    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await entities.FindAsync([id], cancellationToken);
    }

    public Task<int> CountAsync(Expression<Func<T, bool>>? filter = null,CancellationToken cancellationToken = default)
    {
        if (filter is not null)
        {
            return entities.CountAsync(filter, cancellationToken);
        }

        return entities.CountAsync(cancellationToken);
    }
    public void UpdateProperties( T entity,params Expression<Func<T, object?>>[] properties)
    {
        var entry = context.Entry(entity);

        if (entry.State == EntityState.Detached)
        {
            entry.State = EntityState.Unchanged;
        }

        foreach (var property in properties)
        {
            entry.Property(property).IsModified = true;
        }
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await entities.AddAsync(entity, cancellationToken);
    }

    public Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        return this.entities.AddRangeAsync(entities, cancellationToken);
    }
    public void HardDelete(T entity)
    {
        entities.Remove(entity);
    }
    public void DeleteRange(IEnumerable<T> entity)
    {
        entities.RemoveRange(entity);
    }

    
}

