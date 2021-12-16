using Spectre.Console;
using Spectre.Console.Cli;
using TasksTracker.Contracts.Interfaces;
using TasksTracker.Contracts.Models;

namespace TasksTracker.Cli.Commands;

public abstract class BaseCommand<TSettings> : Command<TSettings> where TSettings : CommandSettings
{
    protected readonly ITaskEntryRepository TaskEntryRepository;
    protected readonly List<Tag> TagList;
    protected bool PromptForInput;

    protected BaseCommand(ITaskEntryRepository taskEntryRepository, ITagRepository tagRepository)
    {
        TaskEntryRepository = taskEntryRepository
                               ?? throw new ArgumentNullException(nameof(taskEntryRepository));
        
        TagList = tagRepository.GetList().ToList();
    }
    
    protected IEnumerable<string> PromptTagsIfMissing(string[] tags)
    {
        if (tags != Array.Empty<string>())
        {
            return tags.Length == 1 
                ? tags[0].Trim()
                    .Replace(" ", ",")
                    .Split(",")
                    .Where(t => !string.IsNullOrEmpty(t.Trim()))
                    .ToArray() 
                : tags;
        }

        if (!PromptForInput) 
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
        
        if (!TagList.Any())
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
                    TagList.Select(t => t.Title).ToArray()    
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
}