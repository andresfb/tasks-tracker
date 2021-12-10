using Dapper;
using Humanizer;
using TasksTracker.Contracts.Interfaces;
using TasksTracker.Contracts.Models;
using Microsoft.Extensions.Logging;

namespace TasksTracker.Sqlite.Repositories;

public class TaskEntryRepository : Repository<TaskEntry>, ITaskEntryRepository
{
    private const string GetByCategoryListFile = "GetByCategoryList.sql";
    private const string GetByCategoryFromDateListFile = "GetByCategoryFromDateList.sql";
    private const string GetWithChildrenFile = "GetWithChildren.sql";
    private const string GetBySlugFile = "GetBySlug.sql";

    private readonly ITagRepository _tagRepository;
    private readonly ITaskEntryLinkRepository _linkRepository;
    private readonly ILogger<TaskEntryRepository> _logger;
    
    public TaskEntryRepository(
        IDatabaseContext context, 
        ITagRepository tagRepository, 
        ITaskEntryLinkRepository linkRepository, 
        ILogger<TaskEntryRepository> logger) : base(context)
    {
        _tagRepository = tagRepository;
        _linkRepository = linkRepository;
        _logger = logger;
    }

    public IEnumerable<TaskEntry> GetByCategoryList(Guid categoryId)
    {
        var scripts = GetScriptCollection(nameof(TaskEntry).Pluralize());
        var sql = scripts.GetScriptSql(GetByCategoryListFile);
        using var cnn = Context.GetConnection();
        return cnn.Query<TaskEntry>(sql, new { CategoryId = categoryId });
    }

    public async Task<IEnumerable<TaskEntry>> GetByCategoryListAsync(Guid categoryId)
    {
        var scripts = GetScriptCollection(nameof(TaskEntry).Pluralize());
        var sql = scripts.GetScriptSql(GetByCategoryListFile);
        await using var cnn = Context.GetConnection();
        return await cnn.QueryAsync<TaskEntry>(sql, new { CategoryId = categoryId });
    }

    public IEnumerable<TaskEntry> GetByCategoryFromDateList(Guid categoryId, DateTime? fromDate)
    {
        fromDate ??= DateTime.Today;
        var scripts = GetScriptCollection(nameof(TaskEntry).Pluralize());
        var sql = scripts.GetScriptSql(GetByCategoryFromDateListFile);
        using var cnn = Context.GetConnection();
        return cnn.Query<TaskEntry>(sql, new { CategoryId = categoryId, CreatedAt = fromDate });
    }

    public async Task<IEnumerable<TaskEntry>> GetByCategoryFromDateListAsync(Guid categoryId, DateTime? fromDate)
    {
        fromDate ??= DateTime.Today;
        var scripts = GetScriptCollection(nameof(TaskEntry).Pluralize());
        var sql = scripts.GetScriptSql(GetByCategoryFromDateListFile);
        await using var cnn = Context.GetConnection();
        return await cnn.QueryAsync<TaskEntry>(sql, new { CategoryId = categoryId, CreatedAt = fromDate });
    }

    public TaskEntry? GetBySlug(Guid categoryId, string slug)
    {
        var scripts = GetScriptCollection(nameof(TaskEntry).Pluralize());
        var sql = scripts.GetScriptSql(GetBySlugFile);
        using var cnn = Context.GetConnection();
        return cnn.QueryFirstOrDefault<TaskEntry>(sql, new { CategoryId = categoryId, Slug = slug });
    }

    public async Task<TaskEntry?> GetBySlugAsync(Guid categoryId, string slug)
    {
        var scripts = GetScriptCollection(nameof(TaskEntry).Pluralize());
        var sql = scripts.GetScriptSql(GetBySlugFile);
        await using var cnn = Context.GetConnection();
        return await cnn.QueryFirstOrDefaultAsync<TaskEntry>(sql, new { CategoryId = categoryId, Slug = slug });
    }

    public override TaskEntry? Get(Guid id)
    {
        var scripts = GetScriptCollection(nameof(TaskEntry).Pluralize());
        var sql = scripts.GetScriptSql(GetWithChildrenFile);
        using var cnn = Context.GetConnection();

        using var results = cnn.QueryMultiple(sql, new { Id = id });
        var taskEntry = results.ReadSingleOrDefault<TaskEntry>();
        if (taskEntry == null) return null;

        taskEntry.Tags = results.Read<Tag>()?.ToList() ?? new List<Tag>();
        taskEntry.Links = results.Read<TaskEntryLink>()?.ToList() ?? new List<TaskEntryLink>();

        return taskEntry;
    }

    public override async Task<TaskEntry?> GetAsync(Guid id)
    {
        var scripts = GetScriptCollection(nameof(TaskEntry).Pluralize());
        var sql = scripts.GetScriptSql(GetWithChildrenFile);
        await using var cnn = Context.GetConnection();

        using var results = await cnn.QueryMultipleAsync(sql, new { Id = id });
        var taskEntry = await results.ReadSingleAsync<TaskEntry>();
        if (taskEntry == null) return null;

        taskEntry.Tags = (await results.ReadAsync<Tag>())?.ToList() ?? new List<Tag>();
        taskEntry.Links = (await results.ReadAsync<TaskEntryLink>())?.ToList() ?? new List<TaskEntryLink>();

        return taskEntry;
    }
    

    protected override void Insert(TaskEntry entity)
    {
        ExecuteQuery(UpdateFile, entity);
    }

    protected override async Task InsertAsync(TaskEntry entity)
    {
        await ExecuteQueryAsync(UpdateFile, entity);
    }

    protected override void Update(TaskEntry entity)
    {
        ExecuteQuery(UpdateFile, entity);
    }

    protected override async Task UpdateAsync(TaskEntry entity)
    {
        await ExecuteQueryAsync(UpdateFile, entity);
    }

    
    private void ExecuteQuery(string queryFile, TaskEntry entity)
    {
        var scripts = GetScriptCollection(nameof(TaskEntry).Pluralize());
        var sql = scripts.GetScriptSql(queryFile);
        
        using var cnn = Context.GetConnection();
        cnn.Open();
        using var trans = cnn.BeginTransaction();
        
        try
        {
            cnn.Execute(sql, entity);
            _tagRepository.SaveTagsList(entity.Tags, entity.Id, cnn);
            _linkRepository.SaveLinksList(entity.Links, entity.Id, cnn);
            trans.Commit();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error Saving TaskEntry");
            trans.Rollback();
            throw;
        }
    }

    private async Task ExecuteQueryAsync(string queryFile, TaskEntry entity)
    {
        var scripts = GetScriptCollection(nameof(TaskEntry).Pluralize());
        var sql = scripts.GetScriptSql(queryFile);
        
        await using var cnn = Context.GetConnection();
        await cnn.OpenAsync();
        var trans = await cnn.BeginTransactionAsync();
        
        try
        {
            await cnn.ExecuteAsync(sql, entity);
            await _tagRepository.SaveTagsListAsync(entity.Tags, entity.Id, cnn);
            await _linkRepository.SaveLinksListAsync(entity.Links, entity.Id, cnn);
            await trans.CommitAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error Saving TaskEntry");
            await trans.RollbackAsync();
            throw;
        }
    }
}