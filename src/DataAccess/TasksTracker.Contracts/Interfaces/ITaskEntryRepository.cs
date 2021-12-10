using TasksTracker.Contracts.Models;

namespace TasksTracker.Contracts.Interfaces;

public interface ITaskEntryRepository : IRepository<TaskEntry>
{
    IEnumerable<TaskEntry> GetByCategoryList(Guid categoryId);
    Task<IEnumerable<TaskEntry>> GetByCategoryListAsync(Guid categoryId);
    IEnumerable<TaskEntry> GetByCategoryFromDateList(Guid categoryId, DateTime? fromDate);
    Task<IEnumerable<TaskEntry>> GetByCategoryFromDateListAsync(Guid categoryId, DateTime? fromDate);
    TaskEntry? GetBySlug(Guid categoryId, string slug);
    Task<TaskEntry?> GetBySlugAsync(Guid categoryId, string slug);
}