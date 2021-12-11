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
        [CommandArgument(0, "[Category]")]
        [CommandOption("-c|--category <Category>")]
        [Description("Task Category")]
        public string Category { get; init; } = string.Empty;

        [CommandArgument(1, "[Title]")]
        [CommandOption("-t|--title <Title>")]
        [Description("Task Title")]
        public string Title { get; init; } = string.Empty;

        [CommandArgument(2, "[Note]")]
        [CommandOption("-n|--note <Note>")]
        [Description("Extra Notes")]
        public string Note { get; init; } = string.Empty;

        [CommandArgument(3, "[Tags]")]
        [CommandOption("-g|--tags <Tags>")]
        [Description("Tags")]
        public string[] Tags { get; init; } = Array.Empty<string>();
    }

    private readonly ICategoryRepository _categoryRepository;
    private readonly ITaskEntryRepository _taskEntryRepository;
    private readonly ITagRepository _tagRepository;
    private readonly List<Tag> _tagList;
    
    private bool _promptForInput;
    private Category? _selectedCategory;

    public AddTaskEntryCommand(
        ICategoryRepository categoryRepository, 
        ITaskEntryRepository taskEntryRepository, 
        ITagRepository tagRepository)
    {
        _categoryRepository = categoryRepository 
                              ?? throw new ArgumentNullException(nameof(categoryRepository));
        _taskEntryRepository = taskEntryRepository 
                               ?? throw new ArgumentNullException(nameof(taskEntryRepository));
        _tagRepository = tagRepository
                         ?? throw new ArgumentNullException(nameof(tagRepository));
        
        _tagList = _tagRepository.GetList().ToList();
    }
    
    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        _promptForInput = string.IsNullOrEmpty(settings.Title);
        
        var category = PromptCategoryIfMissing(settings.Category);
        var title = PromptTitleIfMissing(settings.Title);
        var notes = PromptNotesIfMissing(settings.Note);
        var tags = PromptTagsIfMissing(settings.Tags);
        
        var taskEntry = new TaskEntry()
        {
            Status = Status.Created,
            CategoryId = category,
            Title = title.Trim(),
            Notes = notes.Trim(),
        };

        var dataTask = _taskEntryRepository.GetBySlug(category, taskEntry.Slug);
        taskEntry.Id = dataTask?.Id ?? Guid.Empty;
        taskEntry.Tags = SyncTags(tags, dataTask);
        
        taskEntry = _taskEntryRepository.Save(taskEntry);
        var savedEntry = _taskEntryRepository.Get(taskEntry.Id);
        
        AnsiConsole.WriteLine("");
        AnsiConsole.Markup("[green]Task created successfully[/]");
        AnsiConsole.WriteLine("\n");
        var table =new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue)
            .AddColumn(new TableColumn("[u]Category[/]"))
            .AddColumn(new TableColumn("[u]Title[/]"))
            .AddColumn(new TableColumn("[u]Slug[/]"))
            .AddColumn(new TableColumn("[u]Status[/]"))
            .AddColumn(new TableColumn("[u]Notes[/]"))
            .AddColumn(new TableColumn("[u]Tags[/]"))
            .AddColumn(new TableColumn("[u]Created At[/]"))
            .AddColumn(new TableColumn("[u]Updated At[/]"))
            .AddRow(
                $"[green]{_selectedCategory?.Name}[/]", 
                $"[green]{savedEntry?.Title}[/]",
                $"[green]{savedEntry?.Slug}[/]",
                $"[green]{savedEntry?.Status.ToString()}[/]",
                $"[green]{savedEntry?.Notes}[/]",
                $"[green]{string.Join(",", savedEntry?.Tags.Select(t => t.Title).ToList()!)}[/]",
                $"[green]{savedEntry?.CreatedAt}[/]",
                $"[green]{savedEntry?.UpdatedAt}[/]"
            );
        
        AnsiConsole.Write(table);
        return 0;
    }


    private Guid PromptCategoryIfMissing(string current)
    {
        if (!string.IsNullOrEmpty(current))
        {
            _selectedCategory = FindCategory(current);
            if (_selectedCategory != null) return _selectedCategory.Id;
            AnsiConsole.Markup($"[red]Category {current} not found[/]");
            AnsiConsole.WriteLine("\n");
            _promptForInput = true;
        }
        
        var categories = _categoryRepository.GetList().OrderBy(c => c.UpdatedAt).ToList();
        if (!categories.Any())
            throw new ApplicationException("No Categories on record, create at least one");

        if (!_promptForInput)
            return DefaultCategory(categories);
        
        var rule = new Rule("[green]Enter the Task information[/]")
        {
            Alignment = Justify.Left
        };
        
        AnsiConsole.WriteLine("");
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine("");

        if (categories.Count == 1)
        {
            return DefaultCategory(categories);
        }

        _promptForInput = true;
        return categories.Count <= 3 
            ? PromptCategoryChoices(categories) 
            : PromptCategorySelects(categories);
    }

    private Category? FindCategory(string current)
    {
        var category = _categoryRepository.GetByName(current);
        return category ?? null;
    }

    private Guid DefaultCategory(IEnumerable<Category> categories)
    {
        _selectedCategory = categories.First();
        return _selectedCategory.Id;
    }
    
    private Guid PromptCategoryChoices(IReadOnlyCollection<Category> categories)
    {
        var prompt = new TextPrompt<string>("[deepskyblue1]Category[/]")
            .DefaultValue(categories.First().Name)
            .AddChoices(categories.Select(c => c.Name))
            .Validate(title =>
                !string.IsNullOrEmpty(title)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[yellow]Invalid Category[/]")
            );
        
       var select = AnsiConsole.Prompt(prompt);
       _selectedCategory = categories.FirstOrDefault(c => c.Name == select);

       return _selectedCategory?.Id 
              ?? throw new ApplicationException("Invalid Category");
    }
    
    private static Guid PromptCategorySelects(IReadOnlyCollection<Category> categories)
    {
        var select = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[deepskyblue1]Category[/]")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more fruits)[/]")
                .AddChoices(categories.Select(c => c.Name)));
        
        return categories.First(c => c.Name == select).Id;
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
    
    private IEnumerable<Tag> SyncTags(IEnumerable<string> tags, TaskEntry? dataTask)
    {
        if (dataTask == null) 
            return GenTags(tags);

        if (!dataTask.Tags.Any()) 
            return GenTags(tags);

        var result = tags.Where(tag => dataTask.Tags.FirstOrDefault(t => t.Title.ToLower() == tag.ToLower()) != null)
            .ToList();

        return GenTags(result);
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
}