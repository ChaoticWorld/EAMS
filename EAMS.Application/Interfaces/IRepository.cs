using EAMS.Domain.Abstractions;
using System.Linq.Expressions;

namespace EAMS.Application.Interfaces;

/// <summary>
/// 通用仓储接口
/// </summary>
public interface IRepository<T, TKey> where T : FullAuditedEntity<TKey>
{
    Task<T?> GetByIdAsync(TKey id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> FindAsync(Expression<Func<T, bool>> predicate);
    Task<IEnumerable<T>> FindListAsync(Expression<Func<T, bool>> predicate);
    IQueryable<T> GetQueryable();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task DeleteAsync(TKey id);
    Task SoftDeleteAsync(TKey id, string deletedBy);
    Task<bool> ExistsAsync(TKey id);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
}
