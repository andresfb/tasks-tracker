using TasksTracker.Contracts.Models;

namespace TasksTracker.Contracts.Interfaces;

public interface ITaskEntryRepository : IRepository<TaskEntry>
{
    IEnumerable<TaskEntry> GetFromDateList(DateTime? fromDate);
    Task<IEnumerable<TaskEntry>> GetFromDateListAsync(DateTime? fromDate);
    TaskEntry? GetBySlug(string slug);
    Task<TaskEntry?> GetBySlugAsync(string slug);
    
    TaskEntry? GetBySlugToday(string slug);
    Task<TaskEntry?> GetBySlugTodayAsync(string slug);
    Guid ExistsToday(string slug);
    Task<Guid> ExistsTodayAsync(string slug);
}