using System.Text.Json;

namespace FSharpKeywordTranslator;

public class LanguageConfigurationManager
{
    private Dictionary<string, LanguageConfiguration> database = new();

    public LanguageConfiguration LanguageConfiguration { get; set; }
    public EventHandler OnLanguageChanged;

    public LanguageConfiguration GetLanguageConfiguration(string language)
    {
        var assembly = typeof(LanguageConfigurationManager).Assembly;
        using var patchStream = assembly.GetManifestResourceStream($"FSharpKeywordTranslator.Core.keywords.{language}.json") ?? throw new InvalidDataException($"The keyword for F# compiler in language {language} is missing from the assembly.");
        var languageConfiguration = JsonSerializer.Deserialize<LanguageConfiguration>(patchStream, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return languageConfiguration ?? new();
    }

    public Task<LanguageConfiguration> GetLanguageConfigurationAsync(string language)
    {
        return Task.Run(() => GetLanguageConfiguration(language));
    }

    public async Task SetLanguageAsync(string language)
    {
        LanguageConfiguration = await GetLanguageAsync(language);
        OnLanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    public Task LoadDatabaseAsync()
    {
        return Task.Run(() => LoadDatabase());
    }

    public void LoadDatabase()
    {
        foreach (var lang in WellKnownConstants.Languages)
        {
            var keywords = GetLanguageConfiguration(lang.Code);
            database[lang.Code] = keywords ?? new();
        }
    }

    public LanguageConfiguration GetLanguage(string language)
    {
        if (database.Count == 0)
            LoadDatabase();

        return database[language];
    }

    public Task<LanguageConfiguration> GetLanguageAsync(string language)
    {
        return Task.Run(() => GetLanguage(language));
    }
}
