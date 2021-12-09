using System.Data;
using TasksTracker.Contracts.Models;

namespace TasksTracker.Contracts.Interfaces;

public interface ITagRepository : IRepository<Tag>
{
    void SaveTagsList(IEnumerable<Tag> tags, Guid parentId, IDbConnection cnn);
    Task SaveTagsListAsync(IEnumerable<Tag> tags, Guid parentId, IDbConnection cnn);
}