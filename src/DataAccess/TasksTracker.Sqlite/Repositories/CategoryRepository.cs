using Dapper;
using Humanizer;
using TasksTracker.Contracts.Interfaces;
using TasksTracker.Contracts.Models;

namespace TasksTracker.Sqlite.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    private const string GetListWithEntriesFile = "GetWithEntries.txt";
    
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
}