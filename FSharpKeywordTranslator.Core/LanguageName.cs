namespace FSharpKeywordTranslator;

public record LanguageName(string Code, string Country, string Name)
{
    public string? ReplLink { get; set; }
};
