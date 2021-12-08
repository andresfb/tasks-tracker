using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using TasksTracker.Contracts.Interfaces;

namespace TasksTracker.Sqlite;

public class DatabaseContext : IDatabaseContext
{
    public string ConnectionString { get; }

    public DatabaseContext(IConfiguration config)
    {
        ConnectionString = config.GetSection("DatabaseSettings").GetValue<string>("Sqlite");
    }
    
    public DbConnection GetConnection()
    {
        return new SqliteConnection(ConnectionString);
    }
}