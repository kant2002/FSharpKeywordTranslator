namespace FSharpKeywordTranslator;

public static class WellKnownConstants
{
    public static readonly LanguageName[] Languages = new LanguageName[]
    { 
        new("en","us","English"), 
        new("uk","uk","Ukrainian") { ReplLink = "https://kant2002.github.io/fable-repl-ua/" },
        new("tr","tr","Turkish") { ReplLink = "https://kant2002.github.io/fable-repl-tr/" },
        new("kz","kz","Kazakh") { ReplLink = "https://kant2002.github.io/fable-repl-kz/" },
        new("es","es","Spanish"),
        new("de","de","German"),
        new("it","it","Italian"),
        new("pl","pl","Polish"),
        new("cs","cz","Czech"),
        new("nl","nl","Dutch"),
        new("ru","ru","Russian") { ReplLink = "https://kant2002.github.io/fable-repl-ru/" },
    };
}
