using System.Data;
using Dapper;
using Humanizer;
using TasksTracker.Contracts.Interfaces;
using TasksTracker.Contracts.Models;

namespace TasksTracker.Sqlite.Repositories;

public class TaskEntryLinkRepository : Repository<TaskEntryLink>, ITaskEntryLinkRepository
{
    public TaskEntryLinkRepository(IDatabaseContext context) : base(context)
    { }

    public void SaveLinksList(IEnumerable<TaskEntryLink> link, Guid parentId, IDbConnection cnn)
    {
        var list = link.ToList();
        if (!list.Any())
        {
            return;
        }
        
        var sorted = SortLists(list);

        InsertLinks(sorted.ToInsert, parentId, cnn);
        UpdateLinks(sorted.ToUpdate, cnn);
    }
    
    public async Task SaveLinksListAsync(IEnumerable<TaskEntryLink> link, Guid parentId, IDbConnection cnn)
    {
        var list = link.ToList();
        if (!list.Any())
        {
            return;
        }
        
        var sorted = SortLists(list);
        
        await InsertLinksAsync(sorted.ToInsert, parentId, cnn);
        await UpdateLinksAsync(sorted.ToUpdate, cnn);
    }


    private void InsertLinks(List<TaskEntryLink> links, Guid parentId, IDbConnection cnn)
    {
        var scripts = GetScriptCollection(nameof(TaskEntryLink).Pluralize());
        var sql = scripts.GetScriptSql(InsertFile);
        foreach (var link in links)
        {
            link.TaskEntryId = parentId;
        }
        
        cnn.Execute(sql, links);
    }
    
    private async Task InsertLinksAsync(List<TaskEntryLink> links, Guid parentId, IDbConnection cnn)
    {
        var scripts = GetScriptCollection(nameof(TaskEntryLink).Pluralize());
        var sql = scripts.GetScriptSql(InsertFile);
        foreach (var link in links)
        {
            link.TaskEntryId = parentId;
        }
        
        await cnn.ExecuteAsync(sql, links);
    }
    
    private void UpdateLinks(List<TaskEntryLink> links, IDbConnection cnn)
    {
        var scripts = GetScriptCollection(nameof(TaskEntryLink).Pluralize());
        var sql = scripts.GetScriptSql(UpdateFile);
        foreach (var link in links)
        {
            cnn.Execute(sql, link);
        }
    }
    
    private async Task UpdateLinksAsync(List<TaskEntryLink> links, IDbConnection cnn)
    {
        var scripts = GetScriptCollection(nameof(TaskEntryLink).Pluralize());
        var sql = scripts.GetScriptSql(UpdateFile);
        foreach (var link in links)
        {
            await cnn.ExecuteAsync(sql, link);
        }
    }
}