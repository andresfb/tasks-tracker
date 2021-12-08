using Dapper;
using Humanizer;
using TasksTracker.Contracts.Interfaces;
using TasksTracker.Contracts.Models;

namespace TasksTracker.Sqlite.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    private const string GetWithEntriesFile = "GetWithEntries.txt";
    
    public CategoryRepository(IDatabaseContext context) : base(context)
    { }

    public IEnumerable<Category> GetWithEntries()
    {
        var scripts = GetScriptCollection(nameof(Category).Pluralize());
        var sql = scripts.GetScriptSql(GetWithEntriesFile);
        using var cnn = Context.GetConnection();
        return cnn.Query<Category>(sql);
    }

    public async Task<IEnumerable<Category>> GetWithEntriesAsync()
    {
        var scripts = GetScriptCollection(nameof(Category).Pluralize());
        var sql = scripts.GetScriptSql(GetWithEntriesFile);
        await using var cnn = Context.GetConnection();
        return await cnn.QueryAsync<Category>(sql);
    }

    
    protected override void Insert(Category entity)
    {
        var scripts = GetScriptCollection(nameof(Category).Pluralize());
        var sql = scripts.GetScriptSql(InsertFile);
        using var cnn = Context.GetConnection();
        cnn.Execute(sql, entity);
    }

    protected override async Task InsertAsync(Category entity)
    {
        var scripts = GetScriptCollection(nameof(Category).Pluralize());
        var sql = scripts.GetScriptSql(InsertFile);
        await using var cnn = Context.GetConnection();
        await cnn.ExecuteAsync(sql, entity);
    }

    protected override void Update(Category entity)
    {
        var scripts = GetScriptCollection(nameof(Category).Pluralize());
        var sql = scripts.GetScriptSql(UpdateFile);
        using var cnn = Context.GetConnection();
        cnn.Execute(sql, entity);
    }

    protected override async Task UpdateAsync(Category entity)
    {
        var scripts = GetScriptCollection(nameof(Category).Pluralize());
        var sql = scripts.GetScriptSql(UpdateFile);
        await using var cnn = Context.GetConnection();
        await cnn.ExecuteAsync(sql, entity);
    }
}