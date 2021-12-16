using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using TasksTracker.Contracts.Interfaces;

namespace TasksTracker.Cli.Commands;

public class ListTaskEntriesCommand : BaseCommand<ListTaskEntriesCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[Prompt]")]
        [CommandOption("-p|--prompt <Prompt>")]
        [DefaultValue(false)]
        [Description("Prompt for filters")]
        public bool Prompt { get; init; } = false;
        
        [CommandArgument(1, "[From]")]
        [CommandOption("-f|--from <From>")]
        [Description("List From Date")]
        public string? FromDate { get; init; } = string.Empty;

        [CommandArgument(1, "[To]")]
        [CommandOption("-t|--to <To>")]
        [Description("List To Date")]
        public string? ToDate { get; init; } = string.Empty;
        
        [CommandArgument(1, "[Tags]")]
        [CommandOption("-g|--tags <Tags>")]
        [Description("Tags")]
        public string[] Tags { get; init; } = Array.Empty<string>();
    }
    
    public ListTaskEntriesCommand(ITaskEntryRepository taskEntryRepository, ITagRepository tagRepository)
        : base(taskEntryRepository, tagRepository)
    { }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        PromptForInput = settings.Prompt;
        if (PromptForInput)
        {
            var rule = new Rule("[green]Enter the filter values[/]")
            {
                Alignment = Justify.Left
            };

            AnsiConsole.WriteLine("");
            AnsiConsole.Write(rule);
            AnsiConsole.WriteLine("");
        }

        var fromDate = PromptForInput 
            ? PromptFromDate() 
            : ParseFromDate(settings.FromDate);
        var toDate = PromptForInput
            ? PromptToDate()
            : ParseToDate(settings.ToDate);
        var tags = PromptTagsIfMissing(settings.Tags);

        if (fromDate > toDate)
        {
            AnsiConsole.WriteLine("");
            AnsiConsole.Markup("[yellow]Invalid date range[/]");
            AnsiConsole.WriteLine("\n");
            return 0;
        }
        
        var tasks = TaskEntryRepository.GetDateRangeTagsList(fromDate, toDate, tags).ToList();
        if (!tasks.Any())
        {
            AnsiConsole.WriteLine("");
            AnsiConsole.Markup("[yellow]No Tasks found[/]");
            AnsiConsole.WriteLine("\n");
            return 0;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue)
            .AddColumn(new TableColumn("[u]Title[/]"))
            .AddColumn(new TableColumn("[u]Slug[/]"))
            .AddColumn(new TableColumn("[u]Status[/]"))
            .AddColumn(new TableColumn("[u]Notes[/]"))
            .AddColumn(new TableColumn("[u]Created At[/]"))
            .AddColumn(new TableColumn("[u]Updated At[/]"));
        
        foreach (var task in tasks)
        {
            table.AddRow(
                $"[green]{task?.Title}[/]",
                $"[green]{task?.Slug}[/]",
                $"[green]{task?.Status.ToString()}[/]",
                $"[green]{task?.Notes}[/]",
                $"[green]{task?.CreatedAt}[/]",
                $"[green]{task?.UpdatedAt}[/]"    
            );        
        }
        
        AnsiConsole.WriteLine("");
        AnsiConsole.Write(table);
        return 0;
    }

    private DateTime ParseFromDate(string? current)
    {
        if (string.IsNullOrEmpty(current)) 
            return DateTime.Today;
        
        if (DateTime.TryParse(current, out var fromDate))
            return fromDate;

        AnsiConsole.WriteLine("");
        AnsiConsole.Markup("[yellow]Invalid Date[/]");
        AnsiConsole.WriteLine("\n");
        
        PromptForInput = true;
        return PromptFromDate();
    }

    private DateTime? ParseToDate(string? current)
    {
        if (string.IsNullOrEmpty(current)) 
            return null;
        
        if (DateTime.TryParse(current, out var toDate))
            return toDate;

        AnsiConsole.WriteLine("");
        AnsiConsole.Markup("[yellow]Invalid Date[/]");
        AnsiConsole.WriteLine("\n");
        
        PromptForInput = true;
        return PromptToDate();
    }
    
    private static DateTime PromptFromDate()
    {
        var entryValue = AnsiConsole.Prompt(
            new TextPrompt<string>("[deepskyblue1]From Date:[/]")
                .Validate(from =>
                    !string.IsNullOrEmpty(from)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[yellow]Invalid Date[/]")
                )
                .Validate(from =>
                    DateTime.TryParse(from, out _)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[yellow]Invalid Date[/]")
                )
                .DefaultValue(DateTime.Today.ToString("d"))
            );
        
        return DateTime.Parse(entryValue);
    }

    private static DateTime? PromptToDate()
    {
        var entryValue = AnsiConsole.Prompt(
            new TextPrompt<string>("[grey53][[Optional]][/] [deepskyblue1]To Date:[/]").AllowEmpty()
        );

        if (string.IsNullOrEmpty(entryValue.Trim()))
            return null;
        
        if (DateTime.TryParse(entryValue.Trim(), out var toDate)) 
            return toDate;
        
        AnsiConsole.WriteLine("");
        AnsiConsole.Markup("[yellow]Invalid Date. Ignoring[/]");
        AnsiConsole.WriteLine("\n");
        return null;
    }
}