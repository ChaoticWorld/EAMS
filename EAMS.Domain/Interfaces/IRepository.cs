using System.Linq.Expressions;

namespace EAMS.Domain.Interfaces;

/// <summary>
/// 通用仓储接口
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}

/// <summary>
/// 软删除仓储接口
/// </summary>
public interface ISoftDeleteRepository<T> : IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(string id, bool includeDeleted = false);
    Task<IEnumerable<T>> GetAllAsync(bool includeDeleted = false);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, bool includeDeleted = false);
    Task PermanentDeleteAsync(T entity);
}
