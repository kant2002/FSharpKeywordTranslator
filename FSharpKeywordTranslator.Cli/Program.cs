using FSharpKeywordTranslator;
using FSharpKeywordTranslator.Core;
using System.CommandLine;

var langOption = new Option<string>(
    name: "--lang",
    description: "Language to produce patch for.");

var tfmOption = new Option<string>(
    name: "--tfm",
    description: "Target framework to produce patch for. Default net11",
    getDefaultValue: () => "net11");

var rootCommand = new RootCommand("F# localization patch builder");
var fsharpCommand = new Command("fsharp", "Produce patch for F# compiler.")
{
    tfmOption,
    langOption
};
rootCommand.AddCommand(fsharpCommand);
fsharpCommand.SetHandler(ProduceFSharpLocalizationPatch, tfmOption, langOption);
var fableCommand = new Command("fable", "Produce patch for Fable F# fork.")
{
    tfmOption,
    langOption
};
rootCommand.AddCommand(fableCommand);
fableCommand.SetHandler(ProduceSimpleFSharpLocalizationPatch, tfmOption, langOption);
var fableBuildCommand = new Command("fable-build", "Produce patch for Fable F# fork build files.")
{
    tfmOption,
    langOption
};
rootCommand.AddCommand(fableBuildCommand);
fableBuildCommand.SetHandler(ProduceFableFSharpBuildLocalizationPatch, tfmOption, langOption);
var replCommand = new Command("repl", "Produce patch for Fable REPL.")
{
    tfmOption,
    langOption
};
rootCommand.AddCommand(replCommand);
replCommand.SetHandler(ProduceFableReplLocalizationPatch, tfmOption, langOption);
var replColorizationCommand = new Command("repl-colorization", "Produce patch for Fable REPL colorization.")
{
    langOption
};
rootCommand.AddCommand(replColorizationCommand);
replColorizationCommand.SetHandler(ProduceFableReplColorizationPatch, langOption);

return await rootCommand.InvokeAsync(args);

static void ProduceFSharpLocalizationPatch(string tfm, string lang)
{
    tfm = tfm.Replace(".0", "");
    var patchGenerator = new PatchGenerator();
    var l = new LanguageConfigurationManager();
    var configuration = l.GetLanguageConfiguration(lang);
    var patch = patchGenerator.GenerateFSharpPatch(tfm, configuration);
    Console.WriteLine(patch);
}

static void ProduceSimpleFSharpLocalizationPatch(string tfm, string lang)
{
    tfm = tfm.Replace(".0", "");
    var patchGenerator = new PatchGenerator();
    var l = new LanguageConfigurationManager();
    var configuration = l.GetLanguageConfiguration(lang);
    var patch = patchGenerator.GenerateSimpleFSharpPatch(tfm, configuration);
    Console.WriteLine(patch);
}

static void ProduceFableFSharpBuildLocalizationPatch(string tfm, string lang)
{
    tfm = tfm.Replace(".0", "");
    var patchGenerator = new PatchGenerator();
    var l = new LanguageConfigurationManager();
    var configuration = l.GetLanguageConfiguration(lang);
    var patch = patchGenerator.GenerateFableFSharpBuildPatch(tfm, configuration);
    Console.WriteLine(patch);
}

static void ProduceFableReplLocalizationPatch(string tfm, string lang)
{
    tfm = tfm.Replace(".0", "");
    var patchGenerator = new PatchGenerator();
    var l = new LanguageConfigurationManager();
    var configuration = l.GetLanguageConfiguration(lang);
    var patch = patchGenerator.GenerateFableReplPatch(tfm, configuration);
    Console.WriteLine(patch);
}

static void ProduceFableReplColorizationPatch(string lang)
{
    var patchGenerator = new PatchGenerator();
    var l = new LanguageConfigurationManager();
    var configuration = l.GetLanguageConfiguration(lang);
    var patch = patchGenerator.GenerateFableReplColorizationPatch(configuration);
    Console.WriteLine(patch);
}
