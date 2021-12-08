namespace TasksTracker.Contracts.Models;

public class TaskEntryLink : EntityBase
{
    public Guid TaskEntryId { get; set; }
    public string Link { get; set; } = string.Empty;
}