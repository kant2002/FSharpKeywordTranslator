using FSharpKeywordTranslator;
using FSharpKeywordTranslator.Core;
using System.CommandLine;

var langOption = new Option<string>(
    name: "--lang",
    description: "Language to produce patch for.");

var rootCommand = new RootCommand("F# localization patch builder");
var fsharpCommand = new Command("fsharp", "Produce patch for F# compiler.")
{
    langOption
};
rootCommand.AddCommand(fsharpCommand);
fsharpCommand.SetHandler(ProduceFSharpLocalizationPatch, langOption);
var fableCommand = new Command("fable", "Produce patch for Fable REPL.")
{
    langOption
};
rootCommand.AddCommand(fableCommand);
fableCommand.SetHandler(ProduceFableReplLocalizationPatch, langOption);

return await rootCommand.InvokeAsync(args);

static void ProduceFSharpLocalizationPatch(string lang)
{
    var patchGenerator = new PatchGenerator();
    var l = new LanguageConfigurationManager();
    var configuration = l.GetLanguageConfiguration(lang);
    var patch = patchGenerator.GenerateFSharpPatch(configuration);
    Console.WriteLine(patch);
}

static void ProduceFableReplLocalizationPatch(string lang)
{
    var patchGenerator = new PatchGenerator();
    var l = new LanguageConfigurationManager();
    var configuration = l.GetLanguageConfiguration(lang);
    var patch = patchGenerator.GenerateFableReplPatch(configuration);
    Console.WriteLine(patch);
}
