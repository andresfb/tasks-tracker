using System.Data.Common;

namespace TasksTracker.Contracts.Interfaces;

public interface IDatabaseContext
{
    string ConnectionString { get; }

    DbConnection GetConnection();   
}