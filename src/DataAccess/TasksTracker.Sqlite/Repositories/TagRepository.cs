using System.Data;
using Dapper;
using Humanizer;
using TasksTracker.Contracts.Interfaces;
using TasksTracker.Contracts.Models;

namespace TasksTracker.Sqlite.Repositories;

public class TagRepository : Repository<Tag>, ITagRepository
{
    private const string InsertTasksTagsFile = "InsertTasksTags.sql";
    
    public TagRepository(IDatabaseContext context) : base(context)
    { }

    public void SaveTagsList(IEnumerable<Tag> tags, Guid parentId, IDbConnection cnn)
    {
        var list = tags.ToList();
        if (!list.Any())
        {
            return;
        }

        var sorted = SortLists(list);

        InsertTags(sorted.ToInsert, cnn);
        UpdateTags(sorted.ToUpdate, cnn);
        SaveTagsEntries(sorted.Combined, parentId, cnn);
    }

    public async Task SaveTagsListAsync(IEnumerable<Tag> tags, Guid parentId, IDbConnection cnn)
    {
        var list = tags.ToList();
        if (!list.Any())
        {
            return;
        }

        var sorted = SortLists(list);

        await InsertTagsAsync(sorted.ToInsert, cnn);
        await UpdateTagsAsync(sorted.ToUpdate, cnn);
        await SaveTagsEntriesAsync(sorted.Combined, parentId, cnn);
    }
    
    
    private void InsertTags(List<Tag> tags, IDbConnection cnn)
    {
        var scripts = GetScriptCollection(nameof(Tag).Pluralize());
        var sql = scripts.GetScriptSql(InsertFile);
        cnn.Execute(sql, tags);
    }
    
    private async Task InsertTagsAsync(List<Tag> tags, IDbConnection cnn)
    {
        var scripts = GetScriptCollection(nameof(Tag).Pluralize());
        var sql = scripts.GetScriptSql(InsertFile);
        await cnn.ExecuteAsync(sql, tags);
    }
    
    private void UpdateTags(List<Tag> tags, IDbConnection cnn)
    {
        var scripts = GetScriptCollection(nameof(Tag).Pluralize());
        var sql = scripts.GetScriptSql(UpdateFile);
        foreach (var tag in tags)
        {
            cnn.Execute(sql, tag);
        }
    }
    
    private async Task UpdateTagsAsync(List<Tag> tags, IDbConnection cnn)
    {
        var scripts = GetScriptCollection(nameof(Tag).Pluralize());
        var sql = scripts.GetScriptSql(UpdateFile);
        foreach (var tag in tags)
        {
           await cnn.ExecuteAsync(sql, tag);
        }
    }
    
    private void SaveTagsEntries(IEnumerable<Tag> tags, Guid parentId, IDbConnection cnn)
    {
        var scripts = GetScriptCollection(nameof(Tag).Pluralize());
        var sql = scripts.GetScriptSql(InsertTasksTagsFile);

        foreach (var tag in tags.Where(tag => tag.GlueId != Guid.Empty))
        {
            cnn.Execute(sql, new
            {
                Id = Guid.NewGuid(),
                TaskEntryId = parentId,
                TagId = tag.Id,
            });
        }
    }
    
    private async Task SaveTagsEntriesAsync(IEnumerable<Tag> tags, Guid parentId, IDbConnection cnn)
    {
        var scripts = GetScriptCollection(nameof(Tag).Pluralize());
        var sql = scripts.GetScriptSql(InsertTasksTagsFile);
        
        foreach (var tag in tags.Where(tag => tag.GlueId != Guid.Empty))
        {
            await cnn.ExecuteAsync(sql, new
            {
                Id = Guid.NewGuid(),
                TaskEntryId = parentId,
                TagId = tag.Id,
            });
        }
    }
}