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
        [CommandOption("-c|--category <CATEGORY>")]
        [Description("Task Category")]
        public string Category { get; set; } = string.Empty;

        [CommandOption("-t|--title <TITLE>")]
        [Description("Task Title")]
        public string Title { get; set; } = string.Empty;

        [CommandOption("-n|--note <TITLE>")]
        [Description("Extra Notes")]
        public string Notes { get; set; } = string.Empty;
    }

    private readonly ICategoryRepository _categoryRepository;
    private readonly ITaskEntryRepository _taskEntryRepository;
    
    private bool _promptForInput;
    private Category? _selectedCategory;

    public AddTaskEntryCommand(ICategoryRepository categoryRepository, ITaskEntryRepository taskEntryRepository)
    {
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _taskEntryRepository = taskEntryRepository ?? throw new ArgumentNullException(nameof(taskEntryRepository));
    }
    
    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        _promptForInput = string.IsNullOrEmpty(settings.Title);
        
        var category = PromptCategoryIfMissing(settings.Category);
        var title = PromptTitleIfMissing(settings.Title);
        var notes = PromptNotesIfMissing(settings.Notes);
        
        var taskEntry = new TaskEntry()
        {
            Status = Status.Created,
            CategoryId = category,
            Title = title.Trim(),
            Notes = notes.Trim(),
        };

        var dataEntry = _taskEntryRepository.GetBySlug(category, taskEntry.Slug);
        taskEntry.Id = dataEntry?.Id ?? Guid.Empty;
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
        {
            throw new ApplicationException("No Categories on record, create at least one");
        }

        if (!_promptForInput)
        {
            return DefaultCategory(categories);
        }
        
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

       return _selectedCategory?.Id ?? throw new ApplicationException("Invalid Category");
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
        {
            return current;
        }

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
        {
            return current;
        }

        return _promptForInput
            ? AnsiConsole.Prompt(new TextPrompt<string>("[grey53][[Optional]][/] [deepskyblue1]Notes:[/]").AllowEmpty())
            : string.Empty;
    }
}