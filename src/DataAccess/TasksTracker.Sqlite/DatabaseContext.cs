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
        // TODO: change the 'check' table to tags.
        const string checkTable = "Categories";
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
            nameof(Category).Pluralize(),
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
            SeedCategories(cnn);
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
    
    private static void SeedCategories(IDbConnection cnn)
    {
        // TODO: seed two tags (Work, Personal)
        const string insertFile = "Insert.sql";
        var scripts = ScriptCollection.GetScriptCollection(nameof(Category).Pluralize());
        var insertSql = scripts.GetScriptSql(insertFile);

        var category = new Category()
        {
            Id = Guid.NewGuid(),
            Name = "Work",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };
        cnn.Execute(insertSql, category);

        category = new Category()
        {
            Id = Guid.NewGuid(),
            Name = "Personal",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };
        cnn.Execute(insertSql, category);
    }
}