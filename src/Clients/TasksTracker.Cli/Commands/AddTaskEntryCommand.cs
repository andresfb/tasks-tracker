using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using TasksTracker.Contracts.Interfaces;
using TasksTracker.Contracts.Models;
using Status = TasksTracker.Contracts.Enums.Status;

namespace TasksTracker.Cli.Commands;

public class AddTaskEntryCommand : Command<AddTaskEntryCommand.Settings>
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

    private readonly ITaskEntryRepository _taskEntryRepository;
    private readonly List<Tag> _tagList;
    
    private bool _promptForInput;

    public AddTaskEntryCommand(
        ITaskEntryRepository taskEntryRepository, 
        ITagRepository tagRepository)
    {
        _taskEntryRepository = taskEntryRepository 
                               ?? throw new ArgumentNullException(nameof(taskEntryRepository));
        
        _tagList = tagRepository.GetList().ToList();
    }
    
    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        _promptForInput = string.IsNullOrEmpty(settings.Title);
        
        var title = PromptTitleIfMissing(settings.Title);
        var taskEntry = new TaskEntry()
        {
            Status = Status.Created,
            Title = title.Trim(),
        };

        var foundTask = _taskEntryRepository.ExistsToday(taskEntry.Slug);
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
        taskEntry = _taskEntryRepository.Save(taskEntry);
        
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

        _promptForInput = true;
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

        return _promptForInput
            ? AnsiConsole.Prompt(
                new TextPrompt<string>("[grey53][[Optional]][/] [deepskyblue1]Notes:[/]")
                    .AllowEmpty())
            : string.Empty;
    }
    
    private IEnumerable<string> PromptTagsIfMissing(string[] tags)
    {
        if (tags != Array.Empty<string>()) 
            return tags;

        if (!_promptForInput) 
            return tags;

        var response = AnsiConsole.Prompt(
            new TextPrompt<string>("[grey53][[Optional]][/] [deepskyblue1]Tags:[/]")
                .DefaultValue("Show List")
                .AllowEmpty()
        );

        if (string.IsNullOrEmpty(response)) 
            return Array.Empty<string>();

        if (response.Trim().ToLower() != "show list") 
            return response.Split(" ");
        
        if (!_tagList.Any())
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Markup("[red]No tags found[/]\n");
            return PromptForTags();
        }
        
        var results = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("Chose [green]Tags[/] from the list")
                .NotRequired()
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to show more tags)[/]")
                .InstructionsText(
                    "[grey](Press [blue]<space>[/] to toggle a tag, " + 
                    "[green]<enter>[/] to accept)[/]")
                .AddChoices(
                    _tagList.Select(t => t.Title).ToArray()    
                ));

        return results.ToArray();
    }

    private static IEnumerable<string> PromptForTags()
    {
        var response = AnsiConsole.Prompt(
            new TextPrompt<string>("[grey53][[Optional]][/] [deepskyblue1]Tags:[/]")
                .AllowEmpty()
        );

        return string.IsNullOrEmpty(response) 
            ? Array.Empty<string>() 
            : response.Split(" ");
    }

    private IEnumerable<Tag> GenTags(IEnumerable<string> tags)
    {
        var results = new List<Tag>();
        
        foreach (var tag in tags)
        {
            var savedTag = _tagList.FirstOrDefault(t => t.Title.ToLower() == tag.ToLower());
            if (savedTag == null)
            {
                results.Add(new Tag() { Title = tag });
                continue;
            }
            
            results.Add(savedTag);
        }
        
        return results;
    }
    
    private void DisplayEntry(Guid entryId)
    {
        var entry = _taskEntryRepository.Get(entryId);
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