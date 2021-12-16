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
        services.AddSingleton<ListTaskEntriesCommand.Settings>();
        services.AddSingleton<ListTaskEntriesCommand>();
        
        var app = new CommandApp(new TypeRegistrar(services));
        app.Configure(config =>
        {
            config.SetApplicationName("tasker");
            config.SetApplicationVersion("0.0.1");
            config.PropagateExceptions();
            config.AddCommand<AddTaskEntryCommand>("add:task")
                .WithDescription("Create a new Task Entry")
                .WithExample(new[]
                    {"add:task", " The program will prompt you for inputs"}
                )
                .WithExample(new[]
                {
                    "add:task",
                    "--title [-t] \"Respond to Emails\"",
                })
                .WithExample(new[]
                {
                    "add:task",
                    "--title [-t] \"Migrate Database\"",
                    "--note [-n] \"Consult with DBA for access to server\""
                })
                .WithExample(new[]
                {
                    "add:task",
                    "--title [-t] \"Migrate Database\"",
                    "--tags [-g] \"database server migrate\"",
                    " (One word tags)"
                });

            config.AddCommand<ListTaskEntriesCommand>("list:tasks")
                .WithDescription("List the Task Entries")
                .WithExample(new[] 
                    { "list:tasks", " The program will list all Tasks entered today" }
                )
                .WithExample(new[]
                    {
                        "list:tasks",
                        "--prompt [-p] true",
                        " Prompt for all filters"
                    }
                )
                .WithExample(new[]
                {
                    "list:tasks", 
                    "--from [-f] 05/20/2021", 
                    " All tasks starting from the given date"
                })
                .WithExample(new[]
                {
                    "list:tasks", 
                    "--tag [-t] work", 
                    " All tasks entered today and assigned to the given Tag"
                })
                .WithExample(new[]
                {
                    "list:tasks", 
                    "--from [-f] 2021-07-11",
                    "--to [-t] 2021-12-01",
                    "--tag [-g] \"personal, work\"", 
                    " All tasks starting from the given date, to the given date, and assigned to the given Tags"
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
            return -98;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            return -99;
        }
    }
}