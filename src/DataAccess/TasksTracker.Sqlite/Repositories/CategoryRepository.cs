using Dapper;
using Humanizer;
using TasksTracker.Contracts.Interfaces;
using TasksTracker.Contracts.Models;

namespace TasksTracker.Sqlite.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    private const string GetListWithEntriesFile = "GetWithEntries.sql";
    private const string GetByNameFile = "GetByName.sql";
    
    public CategoryRepository(IDatabaseContext context) : base(context)
    { }

    public IEnumerable<Category> GetListWithEntries()
    {
        var scripts = GetScriptCollection(nameof(Category).Pluralize());
        var sql = scripts.GetScriptSql(GetListWithEntriesFile);
        using var cnn = Context.GetConnection();
        return cnn.Query<Category>(sql);
    }

    public async Task<IEnumerable<Category>> GetListWithEntriesAsync()
    {
        var scripts = GetScriptCollection(nameof(Category).Pluralize());
        var sql = scripts.GetScriptSql(GetListWithEntriesFile);
        await using var cnn = Context.GetConnection();
        return await cnn.QueryAsync<Category>(sql);
    }

    public Category? GetByName(string name)
    {
        var scripts = GetScriptCollection(nameof(Category).Pluralize());
        var sql = scripts.GetScriptSql(GetByNameFile);
        using var cnn = Context.GetConnection();
        return cnn.QueryFirstOrDefault<Category>(sql, new { Name = name.Trim().ToLower() });
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        var scripts = GetScriptCollection(nameof(Category).Pluralize());
        var sql = scripts.GetScriptSql(GetByNameFile);
        await using var cnn = Context.GetConnection();
        return await cnn.QueryFirstOrDefaultAsync<Category>(sql, new { Name = name.Trim().ToLower() });
    }
}