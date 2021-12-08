using TasksTracker.Contracts.Models;

namespace TasksTracker.Contracts.Interfaces;

public interface ITaskEntryRepository : IRepository<TaskEntry>
{
    IEnumerable<TaskEntry> GetByCategoryList(Guid categoryId);
    Task<IEnumerable<TaskEntry>> GetByCategoryListAsync(Guid categoryId);
    IEnumerable<TaskEntry> GetTodayList(Guid categoryId);
    Task<IEnumerable<TaskEntry>> GetTodayListAsync(Guid categoryId);
}