using System.Linq.Expressions;
using Learning_Platform_app.Domain;

namespace Learning_Platform_app.Common.Abstractions;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> GetAll(Expression<Func<T, bool>>? filter = null, bool tracking = false);
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? filter = null, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    void UpdateProperties(T entity, params Expression<Func<T, object?>>[] properties);
    void HardDelete(T entity);
    void DeleteRange(IEnumerable<T> entity);
}

