using System.Net.Http.Json;

namespace FSharpKeywordTranslator;

public class LanguageConfigurationManager
{
    private readonly HttpClient http;

    private Dictionary<string, LanguageConfiguration> database = new();

    public LanguageConfigurationManager(HttpClient http)
    {
        this.http = http;
    }

    public LanguageConfiguration LanguageConfiguration { get; set; }
    public EventHandler OnLanguageChanged;

    public async Task<LanguageConfiguration> GetLanguageConfigurationAsync(string language)
    {
        return await http.GetFromJsonAsync<LanguageConfiguration>($"keywords/{language}.json") ?? new();
    }

    public async Task SetLanguageAsync(string language)
    {
        LanguageConfiguration = await GetLanguageAsync(language);
        OnLanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task LoadDatabaseAsync()
    {
        foreach (var lang in WellKnownConstants.Languages)
        {
            var keywords = await GetLanguageConfigurationAsync(lang);
            database[lang] = keywords ?? new();
        }
    }

    public async Task<LanguageConfiguration> GetLanguageAsync(string language)
    {
        if (database.Count == 0)
            await LoadDatabaseAsync();

        return database[language];
    }
}
