using TasksTracker.Contracts.Enums;

namespace TasksTracker.Contracts.Models;

public class TaskEntry
{
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public Status Status { get; set; }
    public string Notes { get; set; } = string.Empty;
    public IEnumerable<TaskEntryLink> Links { get; set; } = new List<TaskEntryLink>();
    public IEnumerable<Tag> Tags { get; set; } = new List<Tag>();
}