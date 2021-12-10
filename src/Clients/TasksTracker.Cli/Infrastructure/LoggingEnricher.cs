using Serilog.Core;
using Serilog.Events;

namespace TasksTracker.Cli.Infrastructure;

internal class LoggingEnricher : ILogEventEnricher
{
#pragma warning disable CS8618
    private string _cachedLogFilePath;
    private LogEventProperty _cachedLogFilePathProperty;
#pragma warning restore CS8618

    public static string Path = string.Empty;

    public const string LogFilePathPropertyName = "LogFilePath";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        LogEventProperty logFilePathProperty;

        if (Path.Equals(_cachedLogFilePath))
        {
            logFilePathProperty = _cachedLogFilePathProperty;
        }
        else
        {
            _cachedLogFilePath = Path;
            _cachedLogFilePathProperty = logFilePathProperty = propertyFactory.CreateProperty(LogFilePathPropertyName, Path);
        }

        logEvent.AddPropertyIfAbsent(logFilePathProperty);
    }
}