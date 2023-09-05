using FSharpKeywordTranslator;
using FSharpKeywordTranslator.Core;
using System.CommandLine;

var langOption = new Option<string>(
    name: "--lang",
    description: "Language to produce patch for.");

var rootCommand = new RootCommand("F# localization patch builder");
var readCommand = new Command("fsharp", "Produce patch for F# compiler.")
{
    langOption
};
rootCommand.AddCommand(readCommand);

readCommand.SetHandler(ProduceFSharpLocalizationPatch, langOption);

return await rootCommand.InvokeAsync(args);

static void ProduceFSharpLocalizationPatch(string lang)
{
    var patchGenerator = new PatchGenerator();
    var l = new LanguageConfigurationManager();
    var configuration = l.GetLanguageConfiguration(lang);
    var patch = patchGenerator.GenerateFSharpPatch(configuration);
    Console.WriteLine(patch);
}
