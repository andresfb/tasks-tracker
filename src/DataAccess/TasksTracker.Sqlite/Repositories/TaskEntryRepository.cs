using Dapper;
using Humanizer;
using TasksTracker.Contracts.Interfaces;
using TasksTracker.Contracts.Models;
using Microsoft.Extensions.Logging;

namespace TasksTracker.Sqlite.Repositories;

public class TaskEntryRepository : Repository<TaskEntry>, ITaskEntryRepository
{
    private const string GetDateRangeListFile = "GetDateRangeList.sql";
    private const string GetDateRangeTagsListFile = "GetDateRangeTagsList.sql";
    private const string GetWithChildrenFile = "GetWithChildren.sql";
    private const string GetBySlugFile = "GetBySlug.sql";
    private const string GetBySlugTodayFile = "GetBySlugToday.sql";
    private const string ExistsTodayFile = "ExistsToday.sql";

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

    public IEnumerable<TaskEntry> GetDateRangeList(DateTime? fromDate, DateTime? toDate)
    {
        fromDate ??= DateTime.Today;
        toDate ??= DateTime.Today.AddYears(10);
        
        var scripts = GetScriptCollection(nameof(TaskEntry).Pluralize());
        var sql = scripts.GetScriptSql(GetDateRangeListFile);
        
        using var cnn = Context.GetConnection();
        return cnn.Query<TaskEntry>(sql, new
        {
            FromDate = fromDate?.Date, 
            ToDate = toDate?.AddDays(1).AddSeconds(-1)
        });
    }

    public async Task<IEnumerable<TaskEntry>> GetDateRangeListAsync(DateTime? fromDate, DateTime? toDate)
    {
        fromDate ??= DateTime.Today;
        toDate ??= DateTime.Today.AddYears(10);
        
        var scripts = GetScriptCollection(nameof(TaskEntry).Pluralize());
        var sql = scripts.GetScriptSql(GetDateRangeListFile);
        
        await using var cnn = Context.GetConnection();
        return await cnn.QueryAsync<TaskEntry>(sql, new
        {
            FromDate = fromDate?.Date, 
            ToDate = toDate?.Date.AddDays(1).AddSeconds(-1)
        });
    }

    public IEnumerable<TaskEntry> GetDateRangeTagsList(DateTime fromDate, DateTime? toDate, IEnumerable<string> tags)
    {
        var tagsList = tags.ToList();
        if (!tagsList.Any())
            return GetDateRangeList(fromDate, toDate);
        
        toDate ??= DateTime.Today.AddYears(10);
        var scripts = GetScriptCollection(nameof(TaskEntry).Pluralize());
        var sql = scripts.GetScriptSql(GetDateRangeTagsListFile);
        using var cnn = Context.GetConnection();
        return cnn.Query<TaskEntry>(sql, new
        {
            FromDate = fromDate.Date, 
            ToDate = toDate?.Date.AddDays(1).AddSeconds(-1), 
            Tags = tagsList
        });
    }

    public async Task<IEnumerable<TaskEntry>> GetDateRangeTagsListAsync(DateTime fromDate, DateTime? toDate, IEnumerable<string> tags)
    {
        var tagsList = tags.ToList();
        if (!tagsList.Any())
            return await GetDateRangeListAsync(fromDate, toDate);
        
        toDate ??= DateTime.Today.AddYears(10);
        var scripts = GetScriptCollection(nameof(TaskEntry).Pluralize());
        var sql = scripts.GetScriptSql(GetDateRangeTagsListFile);
        await using var cnn = Context.GetConnection();
        return await cnn.QueryAsync<TaskEntry>(sql, new
        {
            FromDate = fromDate, 
            ToDate = toDate?.Date.AddDays(1).AddSeconds(-1), 
            Tags = tagsList
        });
    }

    public TaskEntry? GetBySlug(string slug)
    {
        var scripts = GetScriptCollection(nameof(TaskEntry).Pluralize());
        var sql = scripts.GetScriptSql(GetBySlugFile);
        using var cnn = Context.GetConnection();
        return cnn.QueryFirstOrDefault<TaskEntry>(sql, new { Slug = slug });
    }

    public async Task<TaskEntry?> GetBySlugAsync(string slug)
    {
        var scripts = GetScriptCollection(nameof(TaskEntry).Pluralize());
        var sql = scripts.GetScriptSql(GetBySlugFile);
        await using var cnn = Context.GetConnection();
        return await cnn.QueryFirstOrDefaultAsync<TaskEntry>(sql, new { Slug = slug });
    }

    public TaskEntry? GetBySlugToday(string slug)
    {
        var scripts = GetScriptCollection(nameof(TaskEntry).Pluralize());
        var sql = scripts.GetScriptSql(GetBySlugTodayFile);
        using var cnn = Context.GetConnection();
        return cnn.QueryFirstOrDefault<TaskEntry>(sql, new
        {
            Slug = slug,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today.AddDays(1).AddSeconds(-1)
        });
    }

    public async Task<TaskEntry?> GetBySlugTodayAsync(string slug)
    {
        var scripts = GetScriptCollection(nameof(TaskEntry).Pluralize());
        var sql = scripts.GetScriptSql(GetBySlugTodayFile);
        await using var cnn = Context.GetConnection();
        return await cnn.QueryFirstOrDefaultAsync<TaskEntry>(sql, new
        {
            Slug = slug,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today.AddDays(1).AddSeconds(-1)
        });
    }

    public Guid ExistsToday(string slug)
    {
        var scripts = GetScriptCollection(nameof(TaskEntry).Pluralize());
        var sql = scripts.GetScriptSql(ExistsTodayFile);
        using var cnn = Context.GetConnection();
        return cnn.QueryFirstOrDefault<Guid>(sql, new
        {
            Slug = slug,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today.AddDays(1).AddSeconds(-1)
        });
    }

    public async Task<Guid> ExistsTodayAsync(string slug)
    {
        var scripts = GetScriptCollection(nameof(TaskEntry).Pluralize());
        var sql = scripts.GetScriptSql(ExistsTodayFile);
        await using var cnn = Context.GetConnection();
        return await cnn.QueryFirstOrDefaultAsync<Guid>(sql, new
        {
            Slug = slug,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today.AddDays(1).AddSeconds(-1)
        });
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
        ExecuteQuery(InsertFile, entity);
    }

    protected override async Task InsertAsync(TaskEntry entity)
    {
        await ExecuteQueryAsync(InsertFile, entity);
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