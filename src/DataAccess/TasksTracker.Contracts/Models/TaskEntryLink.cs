namespace TasksTracker.Contracts.Models;

public class TaskEntryLink
{
    public Guid TaskEntryId { get; set; }
    public string Link { get; set; } = string.Empty;
}