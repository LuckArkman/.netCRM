namespace Interfaces;

public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(Guid id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<int> SaveChangesAsync();
}