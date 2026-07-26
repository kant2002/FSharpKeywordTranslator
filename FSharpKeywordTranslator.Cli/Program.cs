using FSharpKeywordTranslator;
using FSharpKeywordTranslator.Core;
using System.CommandLine;

var langOption = new Option<string>(
    name: "--lang")
{
    Description = "Language to produce patch for.",
};

var tfmOption = new Option<string>(
    name: "--tfm")
{
    Description = "Target framework to produce patch for. Default net11",
    DefaultValueFactory = (_) => "net11",

};

var fileOption = new Option<FileInfo>(
    name: "--file")
{
    Description = "File which should be translated.",
};

var rootCommand = new RootCommand("F# localization patch builder");
var fsharpCommand = new Command("fsharp", "Produce patch for F# compiler.")
{
    tfmOption,
    langOption
};
rootCommand.Subcommands.Add(fsharpCommand);
fsharpCommand.SetAction((parseResult) => 
    ProduceFSharpLocalizationPatch(
        parseResult.GetRequiredValue(tfmOption),
        parseResult.GetRequiredValue(langOption)));
var fableCommand = new Command("fable", "Produce patch for Fable F# fork.")
{
    tfmOption,
    langOption
};
rootCommand.Subcommands.Add(fableCommand);
fableCommand.SetAction((parseResult) => 
    ProduceSimpleFSharpLocalizationPatch(
        parseResult.GetRequiredValue(tfmOption),
        parseResult.GetRequiredValue(langOption)));
var fableCommand2 = new Command("fable2", "Produce patch for Fable F# fork.")
{
    tfmOption,
    langOption
};
rootCommand.Subcommands.Add(fableCommand2);
fableCommand2.SetAction((parseResult) => 
    ProduceFableFcsFSharpLocalizationPatch(
        parseResult.GetRequiredValue(tfmOption),
        parseResult.GetRequiredValue(langOption)));
var fableBuildCommand = new Command("fable-build", "Produce patch for Fable F# fork build files.")
{
    tfmOption,
    langOption
};
rootCommand.Subcommands.Add(fableBuildCommand);
fableBuildCommand.SetAction((parseResult) => 
    ProduceFableFSharpBuildLocalizationPatch(
        parseResult.GetRequiredValue(tfmOption),
        parseResult.GetRequiredValue(langOption)));
var replCommand = new Command("repl", "Produce patch for Fable REPL.")
{
    tfmOption,
    langOption
};
rootCommand.Subcommands.Add(replCommand);
replCommand.SetAction((parseResult) => 
    ProduceFableReplLocalizationPatch(
        parseResult.GetRequiredValue(tfmOption),
        parseResult.GetRequiredValue(langOption)));
var replColorizationCommand = new Command("repl-colorization", "Produce patch for Fable REPL colorization.")
{
    langOption
};
rootCommand.Subcommands.Add(replColorizationCommand);
replColorizationCommand.SetAction((parseResult) => 
    ProduceFableReplColorizationPatch(
        parseResult.GetRequiredValue(langOption)));

var translateCommand = new Command("translate", "Translate F# code.")
{
    fileOption,
    langOption
};
rootCommand.Subcommands.Add(translateCommand);
translateCommand.SetAction((parseResult) =>
    TranslateFSharpFile(
        parseResult.GetRequiredValue(fileOption),
        parseResult.GetRequiredValue(langOption)));


return rootCommand.Parse(args).Invoke();

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

static void ProduceFableFcsFSharpLocalizationPatch(string tfm, string lang)
{
    tfm = tfm.Replace(".0", "");
    var patchGenerator = new PatchGenerator();
    var l = new LanguageConfigurationManager();
    var configuration = l.GetLanguageConfiguration(lang);
    var patch = patchGenerator.GenerateFableFcsFSharpPatch(tfm, configuration);
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
static void TranslateFSharpFile(FileInfo fileInfo, string language)
{
    var fileTranslator = new FileTranslator();
    var l = new LanguageConfigurationManager();
    var configuration = l.GetLanguageConfiguration(language);
    fileTranslator.TranslateFSharpFile(fileInfo, configuration);
}
