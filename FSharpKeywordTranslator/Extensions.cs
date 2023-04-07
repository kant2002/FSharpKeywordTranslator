namespace FSharpKeywordTranslator;

public static class Extensions
{
    public static string ToUpperCase(this string value)
    {
        return value[0].ToString().ToUpper() + value[1..];
    }
}
