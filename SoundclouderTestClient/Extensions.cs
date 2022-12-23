using System.Text.RegularExpressions;

namespace SoundclouderTestClient;

static partial class Extensions
{
    [GeneratedRegex(@"((http)|(https)):\/\/.+", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex UrlRegex();

    public static bool IsUrl(this ReadOnlySpan<char> str)
    {
        return UrlRegex().IsMatch(str.ToString());
    }

    public static bool IsAlphabetic(this ReadOnlySpan<char> str)
    {
        foreach (var ch in str)
        {
            if (ch < 65 || ch > 122) return false;
        }
        return true;
    }

    public static bool IsNumeric(this ReadOnlySpan<char> str) 
    {
        foreach (var ch in str)
        {
            if (ch < 48 || ch > 57) return false;
        }
        return true;
    }
}