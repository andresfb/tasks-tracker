using TasksTracker.Contracts.Models;

namespace TasksTracker.Contracts.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    IEnumerable<Category> GetListWithEntries();
    Task<IEnumerable<Category>> GetListWithEntriesAsync();
}