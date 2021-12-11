using System.Security.Cryptography;
using System.Text;

namespace TasksTracker.Contracts.Extensions;

public static class HashingString
{
    public static string Sha256Hash(this string source)
    {
        using var sha1Hash = SHA256.Create();
        
        var sourceBytes = Encoding.UTF8.GetBytes(source);
        var hashBytes = sha1Hash.ComputeHash(sourceBytes);
        return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
    }
}