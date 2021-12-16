using System.Globalization;

namespace TasksTracker.Contracts.Models;

public class Tag : EntityBase
{
    private readonly string _title = string.Empty;

    public string Title
    {
        get => _title;
        init
        {
            var textInfo = new CultureInfo("en-US", false).TextInfo;
            _title = textInfo.ToTitleCase(
                value.ToLower()
                    .Replace(",", "")
                    .Replace(".", "")
                    .Replace(";", "")
                    .Replace("|", "")
                    .Trim()
            );
        }
    }

    public bool IsDefault { get; set; } = false;
    
    public Guid GlueId { get; set; } = Guid.Empty;
}