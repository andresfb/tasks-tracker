using System.Globalization;

namespace TasksTracker.Contracts.Models;

public class Tag : EntityBase
{
    private string _title = string.Empty;

    public string Title
    {
        get => _title;
        set
        {
            var textInfo = new CultureInfo("en-US", false).TextInfo;
            _title = textInfo.ToTitleCase(value.ToLower().Trim());
        }
    }

    public Guid GlueId { get; set; } = Guid.Empty;
}