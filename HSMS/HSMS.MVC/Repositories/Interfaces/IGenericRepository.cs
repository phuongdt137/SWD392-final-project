using System.Linq.Expressions;

namespace HSMS.MVC.Repositories.Interfaces;

/// <summary>
/// Giao diện Repository dùng chung cho các thao tác CRUD cơ bản trên thực thể T.
/// </summary>
/// <typeparam name="T">Kiểu thực thể.</typeparam>
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(long id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task<int> SaveChangesAsync();
}
