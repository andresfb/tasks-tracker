using System.Data;
using System.Data.Common;
using Dapper;
using Humanizer;
using Microsoft.Data.Sqlite;
using TasksTracker.Contracts.Interfaces;
using TasksTracker.Contracts.Models;
using TasksTracker.Sqlite.Helpers;

namespace TasksTracker.Sqlite;

public class DatabaseContext : IDatabaseContext
{
    public string ConnectionString { get; }

    public DatabaseContext()
    {
        ConnectionString = $"Data Source={Environment.GetEnvironmentVariable("TASKS_TRACKER_DB_FILE")}";
        BoostrapDatabase();
    }
    
    public DbConnection GetConnection()
    {
        return new SqliteConnection(ConnectionString);
    }
    
    
    private void BoostrapDatabase()
    {
        const string checkTable = "Tags";
        using var cnn = GetConnection();
        {
            var table = cnn.Query<string>($"SELECT name FROM sqlite_master WHERE type='table' AND name = '{checkTable}';");
            var tableName = table.FirstOrDefault();
            if (!string.IsNullOrEmpty(tableName) && tableName == checkTable)
            {
                return;
            }
        }
        
        CreateDatabase();
    }

    private void CreateDatabase()
    {
        var tables = new List<string>
        {
            nameof(TaskEntry).Pluralize(),
            nameof(TaskEntryLink).Pluralize(),
            nameof(Tag).Pluralize()
        };
        
        using var cnn = GetConnection();
        cnn.Open();
        using var transaction = cnn.BeginTransaction();
        
        try
        {
            CreateTables(tables, cnn);
            SeedDatabase(cnn);
            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    private static void CreateTables(IEnumerable<string> tables, IDbConnection cnn)
    {
        const string createFile = "CreateTable.sql";

        foreach (var sql in tables
                     .Select(ScriptCollection.GetScriptCollection)
                     .Select(scripts => scripts.GetScriptSql(createFile)))
        {
            cnn.Execute(sql);
        }
    }
    
    private static void SeedDatabase(IDbConnection cnn)
    {
        const string insertFile = "Insert.sql";
        var scripts = ScriptCollection.GetScriptCollection(nameof(Tag).Pluralize());
        var insertSql = scripts.GetScriptSql(insertFile);

        var category = new Tag()
        {
            Id = Guid.NewGuid(),
            Title = "Personal",
            IsDefault = false,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };
        cnn.Execute(insertSql, category);

        category = new Tag()
        {
            Id = Guid.NewGuid(),
            Title = "Work",
            IsDefault = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };
        cnn.Execute(insertSql, category);
    }
}