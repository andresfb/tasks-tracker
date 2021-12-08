using System.Reflection;
using Dapper;
using Dapper.Scripts.Collection;
using Humanizer;
using TasksTracker.Contracts.Interfaces;
using TasksTracker.Contracts.Models;

namespace TasksTracker.Sqlite.Repositories;

public abstract class Repository<T> : IRepository<T> where T : EntityBase
{
    private const string BaseScriptsFolder = "Base";
    private const string GetListFile = "GetList.sql";
    private const string GetPaginatedListFile = "GetPaginatedList.sql";
    private const string GetRecordFile = "GetRecord.sql";
    private const string DeleteRecordFile = "GetRecord.sql";
    protected const string InsertFile = "Insert.sql";
    protected const string UpdateFile = "Update.sql";
    
    private readonly string _tableName;

    protected readonly IDatabaseContext Context;
    
    protected Repository(IDatabaseContext context)
    {
        Context = context;
        _tableName = nameof(T).Pluralize();
    }

    public IEnumerable<T> GetList()
    {
        var scripts = GetScriptCollection(BaseScriptsFolder);
        var sql = scripts.GetScriptSql(GetListFile, new { TableName = _tableName });
        using var cnn = Context.GetConnection();
        return cnn.Query<T>(sql);
    }

    public async Task<IEnumerable<T>> GetListAsync()
    {
        var scripts = GetScriptCollection(BaseScriptsFolder);
        var sql = scripts.GetScriptSql(GetListFile, new { TableName = _tableName });
        await using var cnn = Context.GetConnection();
        return await cnn.QueryAsync<T>(sql);
    }

    public IEnumerable<T> GetPaginatedList(int pageNumber = 1, int pageSize = 10)
    {
        var scripts = GetScriptCollection(BaseScriptsFolder);
        var sql = scripts.GetScriptSql(GetPaginatedListFile, new
        {
            TableName = _tableName, 
            RowCount = pageSize, 
            Offset = pageSize * (pageNumber - 1) 
        });
        
        using var cnn = Context.GetConnection();
        return cnn.Query<T>(sql);
    }

    public async Task<IEnumerable<T>> GetPaginatedListAsync(int pageNumber = 1, int pageSize = 10)
    {
        var scripts = GetScriptCollection(BaseScriptsFolder);
        var sql = scripts.GetScriptSql(GetPaginatedListFile, new
        {
            TableName = _tableName, 
            RowCount = pageSize, 
            Offset = pageSize * (pageNumber - 1) 
        });
        
        await using var cnn = Context.GetConnection();
        return await cnn.QueryAsync<T>(sql);
    }

    public T Get(Guid id)
    {
        var scripts = GetScriptCollection(BaseScriptsFolder);
        var sql = scripts.GetScriptSql(GetRecordFile, new { TableName = _tableName });
        using var cnn = Context.GetConnection();
        return cnn.QueryFirst<T>(sql, new { Id = id });
    }

    public async Task<T> GetAsync(Guid id)
    {
        var scripts = GetScriptCollection(BaseScriptsFolder);
        var sql = scripts.GetScriptSql(GetRecordFile, new { TableName = _tableName });
        await using var cnn = Context.GetConnection();
        return await cnn.QueryFirstAsync<T>(sql, new { Id = id });
    }

    public void Save(T entity)
    {
        entity.UpdatedAt = DateTime.Now;
        
        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.Now;
            Insert(entity);
            return;
        }
        
        Update(entity);
    }

    public async Task SaveAsync(T entity)
    {
        entity.UpdatedAt = DateTime.Now;
        
        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.Now;
            await InsertAsync(entity);
            return;
        }
        
        await UpdateAsync(entity);
    }

    public void Delete(Guid id)
    {
        var scripts = GetScriptCollection(BaseScriptsFolder);
        var sql = scripts.GetScriptSql(DeleteRecordFile, new { TableName = _tableName });
        using var cnn = Context.GetConnection();
        cnn.Execute(sql, new { Id = id });
    }

    public async Task DeleteAsync(Guid id)
    {
        var scripts = GetScriptCollection(BaseScriptsFolder);
        var sql = scripts.GetScriptSql(DeleteRecordFile, new { TableName = _tableName });
        await using var cnn = Context.GetConnection();
        await cnn.ExecuteAsync(sql, new { Id = id });
    }

    protected abstract void Insert(T entity);
    
    protected abstract Task InsertAsync(T entity);
    
    protected abstract void Update(T entity);
    
    protected abstract Task UpdateAsync(T entity);
    
    protected virtual SqlScriptCollection GetScriptCollection(string baseFolder = "")
    {
        if (string.IsNullOrEmpty(baseFolder)) baseFolder = _tableName;
        
        var scripts = new SqlScriptCollection();
        var assembly = Assembly.GetExecutingAssembly();
        scripts.Add.FromAssembly(assembly, $"TasksTracker.Sqlite.Queries.{baseFolder}");
        
        return scripts;
    }
}