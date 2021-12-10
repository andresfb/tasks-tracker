using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using TasksTracker.Cli.Commands;
using TasksTracker.Cli.Infrastructure;
using Serilog;

namespace TasksTracker.Cli;

public static class AppRunner
{
    public static int Execute(IEnumerable<string> args)
    {
        var services = new ServiceCollection()
            .AddLogging(configure =>
                configure.AddSerilog(new LoggerConfiguration()
                    .Enrich.With<LoggingEnricher>()
                    .WriteTo.Map(LoggingEnricher.LogFilePathPropertyName,
                        (logFilePath, wt) => wt.File($"{logFilePath}"), 1)
                    .CreateLogger()
                )
            );
        
        Sqlite.ServiceRegistration.AddServiceRegistration(services);
        
        services.AddSingleton<AddTaskEntryCommand.Settings>();
        services.AddSingleton<AddTaskEntryCommand>();
        
        var app = new CommandApp(new TypeRegistrar(services));
        app.Configure(config =>
        {
            config.SetApplicationName("tasker");
            config.SetApplicationVersion("0.0.1");
            config.PropagateExceptions();
            config.AddCommand<AddTaskEntryCommand>("add:task")
                .WithDescription("Create a new Task Entry")
                .WithExample(new[]
                    {"add:task", "the program will prompt you for inputs"}
                )
                .WithExample(new[]
                {
                    "add:task",
                    "--title [-t] \"Respond to Emails\"",
                    "(Category will default to the first record found)"
                })
                .WithExample(new[]
                    {"add:task", "--category [-c] personal", "--title [-t] \"Pickup Mail\""}
                )
                .WithExample(new[]
                {
                    "add:task",
                    "--category [-c] work",
                    "--title [-t] \"Migrate Database\"",
                    "--note [-n] \"Consult with DBA for access to server\""
                })
                .WithExample(new[]
                {
                    "add:task",
                    "--title [-t] \"Migrate Database\"",
                    "--tags [-g] \"database server migrate\""
                });
        });

        try
        {
            return app.Run(args);
        }
        catch (CommandParseException e)
        {
            AnsiConsole.WriteLine("");
            AnsiConsole.WriteLine(e.Message);
            return -99;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            return -99;
        }
    }
}