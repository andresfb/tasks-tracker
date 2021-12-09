namespace TasksTracker.Contracts.Models;

public abstract class EntityBase
{
    public Guid Id { get; set; } = Guid.Empty;
    public DateTime DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}