using TasksTracker.Contracts.Models;

namespace TasksTracker.Contracts.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    IEnumerable<Category> GetWithEntries();
    Task<IEnumerable<Category>> GetWithEntriesAsync();
}