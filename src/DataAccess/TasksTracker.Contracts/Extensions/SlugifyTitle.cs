using System.Text;
using Slugify;

namespace TasksTracker.Contracts.Extensions;

public static class SlugifyTitle
{
    private const int CharCount = 5;
    
    public static string Slugify(this string value)
    {
        var hash = GetHash(value);
        var helper = new SlugHelper();
        var slug = helper.GenerateSlug(value.Trim());
        var words = slug.Split('-').Take(3).ToList();
        var result = words.Select(word => new string(word.Take(CharCount).ToArray())).ToList();
        
        result.Add(hash);
        return string.Join("-", result);
    }

    private static string GetHash(string value)
    {
        var combined = $"{value.Trim().ToLower()}:{DateTime.Today:d}".Sha256Hash();
        var hash = Convert.ToBase64String(Encoding.UTF8.GetBytes(combined))
            .Replace("=", "")
            .ToLower();
        
        var result = new List<char>();
        
        foreach (var c in hash.ToCharArray())
        {
            if (result.Count == CharCount)
                break;
            
            if (!char.IsLetter(c))
                continue;

            result.Add(c);
        }

        return string.Join("", result);
    }
}