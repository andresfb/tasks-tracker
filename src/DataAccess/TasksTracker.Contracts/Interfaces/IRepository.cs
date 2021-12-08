using TasksTracker.Contracts.Models;

namespace TasksTracker.Contracts.Interfaces;

public interface IRepository<T> where T : EntityBase
{
    IEnumerable<T> GetList();
    Task<IEnumerable<T>> GetListAsync();
    IEnumerable<T> GetPaginatedList(int pageNumber = 1, int pageSize = 10);
    Task<IEnumerable<T>> GetPaginatedListAsync(int pageNumber = 1, int pageSize = 10);
    T Get(Guid id);
    Task<T> GetAsync(Guid id);
    public T Save(T entity);
    public Task<T> SaveAsync(T entity);
    public void Delete(Guid id);
    public Task DeleteAsync(Guid id);
}