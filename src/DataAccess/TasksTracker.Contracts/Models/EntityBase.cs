namespace TasksTracker.Contracts.Models;

public abstract class EntityBase
{
    public Guid Id { get; set; }
    public DateTime DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}