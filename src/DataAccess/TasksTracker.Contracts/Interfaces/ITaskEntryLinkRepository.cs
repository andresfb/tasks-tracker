using System.Data;
using TasksTracker.Contracts.Models;

namespace TasksTracker.Contracts.Interfaces;

public interface ITaskEntryLinkRepository : IRepository<TaskEntryLink>
{
    void SaveLinksList(IEnumerable<TaskEntryLink> link, Guid parentId, IDbConnection cnn);
    Task SaveLinksListAsync(IEnumerable<TaskEntryLink> link, Guid parentId, IDbConnection cnn);
}