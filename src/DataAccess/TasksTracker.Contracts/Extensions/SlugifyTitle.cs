using System.Text;
using Slugify;

namespace TasksTracker.Contracts.Extensions;

public static class SlugifyTitle
{
    public static string Slugify(this string value)
    {
        var hash = GetHash(value);
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
        result.Add(hash);
        
        return string.Join("-", result);
    }

    private static string GetHash(string value)
    {
        var hash = Convert.ToBase64String(Encoding.UTF8.GetBytes(value.Sha256Hash()))
            .Replace("=", "")
            .ToLower();
        
        var result = new List<char>();
        
        foreach (var c in hash.ToCharArray())
        {
            if (result.Count == 4)
                break;
            
            if (!char.IsLetter(c))
                continue;

            result.Add(c);
        }

        return string.Join("", result);
    }
}