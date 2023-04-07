namespace FSharpKeywordTranslator;

public class LanguageConfiguration
{
    public KeywordsObj Keywords { get; set; } = new();
    public VariablesObj Variables { get; set; } = new();
}
