namespace FSharpKeywordTranslator;

public static class Extensions
{
    public static string ToUpperCase(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return value[0].ToString().ToUpper() + value[1..];
    }
}
