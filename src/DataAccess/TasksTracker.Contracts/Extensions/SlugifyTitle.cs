using System.Text;
using Slugify;

namespace TasksTracker.Contracts.Extensions;

public static class SlugifyTitle
{
    public static string Slugify(this string value)
    {
        var helper = new SlugHelper();
        var slug = helper.GenerateSlug(
            value.Trim()
                .Replace("-", "")
                .Replace("_", "")
                .Replace(",", "")
                .Replace(".", "")
                .Replace(";", "")
        );

        var words = slug.Split('-').Take(3).ToList();
        var result = words.Select(word => new string(word.Take(4).ToArray())).ToList();

        return string.Join(
            "-",
            result
        );
    }
}