namespace FSharpKeywordTranslator;

public class LanguageConfiguration
{
    public string Language { get; set; }
    public KeywordsObj Keywords { get; set; } = new();
    public VariablesObj Variables { get; set; } = new();
}
