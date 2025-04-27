namespace aiExecBackend.Extensions;

public static class CodeComparisonExtensions
{
    private static string RemoveAllWhitespace(this string str)
    {
        return string.Join("", str.Where(c => !char.IsWhiteSpace(c)));
    }

    public static bool ContainsWhenBothStringsHaveWhitespaceRemoved(this string source, string target)
    {
        var normalizedSource = source.RemoveAllWhitespace();
        var normalizedTarget = target.RemoveAllWhitespace();
        
        return normalizedSource.Contains(normalizedTarget);
    }
}