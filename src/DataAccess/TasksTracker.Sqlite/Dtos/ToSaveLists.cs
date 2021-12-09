using TasksTracker.Contracts.Models;

namespace TasksTracker.Sqlite.Dtos;

public class ToSaveLists<T> where T : EntityBase
{
    public List<T> ToInsert { get; } = new();
    public List<T> ToUpdate { get; } = new();
    public List<T> Combined { get; } = new();

    public void Combine()
    {
        Combined.AddRange(ToInsert);
        Combined.AddRange(ToUpdate);
    }
}