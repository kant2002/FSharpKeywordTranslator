using System.Net.Http.Json;

namespace FSharpKeywordTranslator;

public class LanguageConfigurationManager
{
    private readonly HttpClient http;

    public LanguageConfigurationManager(HttpClient http)
    {
        this.http = http;
    }

    public async Task<LanguageConfiguration> GetLanguageConfigurationAsync(string language)
    {
        return await http.GetFromJsonAsync<LanguageConfiguration>($"keywords/{language}.json") ?? new();
    }
}
