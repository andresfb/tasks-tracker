using System.Globalization;
using TasksTracker.Contracts.Enums;
using TasksTracker.Contracts.Extensions;

namespace TasksTracker.Contracts.Models;

public class TaskEntry : EntityBase
{
    public Guid CategoryId { get; set; }

    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set
        {
            var textInfo = new CultureInfo("en-US", false).TextInfo;
            _title = textInfo.ToTitleCase(value.ToLower().Trim());
            Slug = value.Slugify();
        }
    }

    public string Slug { get; private set; } = string.Empty;

    public Status Status { get; set; }
    public string Notes { get; set; } = string.Empty;
    public IEnumerable<TaskEntryLink> Links { get; set; } = new List<TaskEntryLink>();
    public IEnumerable<Tag> Tags { get; set; } = new List<Tag>();
}