namespace TasksTracker.Contracts.Models;

public class Category : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public int EntriesCount { get; set; }
    public IEnumerable<TaskEntry> Entries { get; set; } = new List<TaskEntry>();
}