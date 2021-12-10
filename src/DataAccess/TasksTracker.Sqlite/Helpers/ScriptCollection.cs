using System.Reflection;
using Dapper.Scripts.Collection;

namespace TasksTracker.Sqlite.Helpers;

public static class ScriptCollection
{
    public static SqlScriptCollection GetScriptCollection(string baseFolder)
    {
        var scripts = new SqlScriptCollection();
        var assembly = Assembly.GetExecutingAssembly();
        scripts.Add.FromAssembly(assembly, $"TasksTracker.Sqlite.Queries.{baseFolder}");
        
        return scripts;
    }
}