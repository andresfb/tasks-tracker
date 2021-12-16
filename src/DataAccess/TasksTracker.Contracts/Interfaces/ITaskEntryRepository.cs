using TasksTracker.Contracts.Models;

namespace TasksTracker.Contracts.Interfaces;

public interface ITaskEntryRepository : IRepository<TaskEntry>
{
    IEnumerable<TaskEntry> GetDateRangeList(DateTime? fromDate, DateTime? toDate);
    Task<IEnumerable<TaskEntry>> GetDateRangeListAsync(DateTime? fromDate, DateTime? toDate);
    IEnumerable<TaskEntry> GetDateRangeTagsList(DateTime fromDate, DateTime? toDate, IEnumerable<string> tags);
    Task<IEnumerable<TaskEntry>> GetDateRangeTagsListAsync(DateTime fromDate, DateTime? toDate, IEnumerable<string> tags);
    TaskEntry? GetBySlug(string slug);
    Task<TaskEntry?> GetBySlugAsync(string slug);
    TaskEntry? GetBySlugToday(string slug);
    Task<TaskEntry?> GetBySlugTodayAsync(string slug);
    Guid ExistsToday(string slug);
    Task<Guid> ExistsTodayAsync(string slug);
}