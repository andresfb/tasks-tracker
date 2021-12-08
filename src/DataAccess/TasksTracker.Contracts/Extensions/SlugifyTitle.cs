using Slugify;

namespace TasksTracker.Contracts.Extensions;

public static class SlugifyTitle
{
    public static string Slugify(this string value)
    {
        var helper = new SlugHelper();
        var slug = helper.GenerateSlug(value);

        return string.Join(
            "-",
            slug.Split('-').Take(3)
        );
    }
}