namespace FSharpKeywordTranslator;

public class LanguageConfiguration
{
    public string Language { get; set; }
    public string LanguageName { get; set; }
    public string? ReplLink { get; set; }
    public KeywordsObj Keywords { get; set; } = new();
    public VariablesObj Variables { get; set; } = new();
    public StdLibObj StdLib { get; set; } = new();
}
