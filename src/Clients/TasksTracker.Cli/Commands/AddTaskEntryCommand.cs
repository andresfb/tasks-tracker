using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using TasksTracker.Contracts.Interfaces;
using TasksTracker.Contracts.Models;
using Status = TasksTracker.Contracts.Enums.Status;

namespace TasksTracker.Cli.Commands;

public class AddTaskEntryCommand : BaseCommand<AddTaskEntryCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[Title]")]
        [CommandOption("-t|--title <Title>")]
        [Description("Task Title")]
        public string Title { get; init; } = string.Empty;

        [CommandArgument(1, "[Note]")]
        [CommandOption("-n|--note <Note>")]
        [Description("Extra Notes")]
        public string Note { get; init; } = string.Empty;

        [CommandArgument(2, "[Tags]")]
        [CommandOption("-g|--tags <Tags>")]
        [Description("Tags")]
        public string[] Tags { get; init; } = Array.Empty<string>();
    }

    public AddTaskEntryCommand(ITaskEntryRepository taskEntryRepository, ITagRepository tagRepository) 
        : base(taskEntryRepository, tagRepository)
    { }
    
    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        PromptForInput = string.IsNullOrEmpty(settings.Title);
        
        var title = PromptTitleIfMissing(settings.Title);
        var taskEntry = new TaskEntry()
        {
            Status = Status.Created,
            Title = title.Trim(),
        };

        var foundTask = TaskEntryRepository.ExistsToday(taskEntry.Slug);
        if (foundTask != Guid.Empty)
        {
            AnsiConsole.WriteLine("");
            AnsiConsole.Markup("[yellow]Task already exists[/]");
            AnsiConsole.WriteLine("\n");
            DisplayEntry(foundTask);
            return 0;
        } 
        
        var notes = PromptNotesIfMissing(settings.Note);
        var tags = PromptTagsIfMissing(settings.Tags);

        taskEntry.Notes = notes.Trim();
        taskEntry.Tags = GenTags(tags);
        taskEntry = TaskEntryRepository.Save(taskEntry);
        
        AnsiConsole.WriteLine("");
        AnsiConsole.Markup("[green]Task created successfully[/]");
        AnsiConsole.WriteLine("\n");
        
        DisplayEntry(taskEntry.Id);
        return 0;
    }


    private string PromptTitleIfMissing(string? current)
    {
        if (!string.IsNullOrEmpty(current)) 
            return current;

        var rule = new Rule("[green]Enter the Task information[/]")
        {
            Alignment = Justify.Left
        };
        
        AnsiConsole.WriteLine("");
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine("");
        
        PromptForInput = true;
        return AnsiConsole.Prompt(
            new TextPrompt<string>("[deepskyblue1]Task Title:[/]")
                .Validate(title => 
                    !string.IsNullOrEmpty(title)
                    ? ValidationResult.Success() 
                    : ValidationResult.Error("[yellow]Invalid Title[/]")
                )
            );
    }
    
    private string PromptNotesIfMissing(string? current)
    {
        if (!string.IsNullOrEmpty(current)) 
            return current;

        return PromptForInput
            ? AnsiConsole.Prompt(
                new TextPrompt<string>("[grey53][[Optional]][/] [deepskyblue1]Notes:[/]")
                    .AllowEmpty())
            : string.Empty;
    }

    private IEnumerable<Tag> GenTags(IEnumerable<string> tags)
    {
        var results = new List<Tag>();
        var list = tags.ToList();
        
        if (!list.Any())
        {
            var defaultTag = TagList.First(t => t.IsDefault);
            results.Add(defaultTag);
            return results;
        }
        
        foreach (var tag in list)
        {
            var cleanTag = tag.Trim()
                .Replace(",", "")
                .Replace(".", "")
                .Replace(";", "")
                .ToLower();
            
            var savedTag = TagList.FirstOrDefault(t => t.Title.ToLower() == cleanTag);
            if (savedTag == null)
            {
                results.Add(new Tag() { Title = cleanTag });
                continue;
            }
            
            results.Add(savedTag);
        }
        
        return results;
    }
    
    private void DisplayEntry(Guid entryId)
    {
        var entry = TaskEntryRepository.Get(entryId);
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue)
            .AddColumn(new TableColumn("[u]Title[/]"))
            .AddColumn(new TableColumn("[u]Slug[/]"))
            .AddColumn(new TableColumn("[u]Status[/]"))
            .AddColumn(new TableColumn("[u]Notes[/]"))
            .AddColumn(new TableColumn("[u]Tags[/]"))
            .AddColumn(new TableColumn("[u]Created At[/]"))
            .AddColumn(new TableColumn("[u]Updated At[/]"))
            .AddRow(
                $"[green]{entry?.Title}[/]",
                $"[green]{entry?.Slug}[/]",
                $"[green]{entry?.Status.ToString()}[/]",
                $"[green]{entry?.Notes}[/]",
                $"[green]{string.Join(",", entry?.Tags.Select(t => t.Title).ToList()!)}[/]",
                $"[green]{entry?.CreatedAt}[/]",
                $"[green]{entry?.UpdatedAt}[/]"    
            );
        
        AnsiConsole.Write(table);
    }
}