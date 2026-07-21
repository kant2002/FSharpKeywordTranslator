using System.Text;

namespace FSharpKeywordTranslator.Core;

public class PatchGenerator
{
    public string GenerateFSharpPatch(string tfm, LanguageConfiguration current)
    {
        var patchTemplate = GetFSharpPatchTemplate(tfm);
        return ApplyKeywords(current, patchTemplate);
    }

    public string GenerateFableReplPatch(string tfm, LanguageConfiguration current)
    {
        var patchTemplate = GetFableReplPatchTemplate(tfm, current.Language)
            ??
            """
            ---
            diff --git a/src/App/Prelude.fs b/src/App/Prelude.fs
            index ece83b0..c8ec73d 100644
            --- a/src/App/Prelude.fs
            +++ b/src/App/Prelude.fs
            @@ -15,7 +15,8 @@ module Literals =
                     // "http://localhost:8080"
                     Browser.Dom.window.location.href
             #else
            -        "https://fable.io/repl/"
            +        Browser.Dom.window.location.href
            +        //"https://fable.io/repl/"
             #endif
                 printfn $"HOST {HOST}"
             
            -- 
            2.37.1.windows.1
            """.ReplaceLineEndings("\n");
        return patchTemplate;
    }

    public string GenerateSimpleFSharpPatch(string tfm, LanguageConfiguration current)
    {
        var patchTemplate = GetFableFSharpPatchTemplate(tfm);
        return ApplyKeywords(current, patchTemplate);
    }

    public string GenerateFableFSharpBuildPatch(string tfm, LanguageConfiguration current)
    {
        var patchTemplate = GetFableFSharpBuildPatchTemplate(tfm);
        return patchTemplate;
    }

    public string GenerateFableReplColorizationPatch(LanguageConfiguration current)
    {
        var patchTemplate = GetFableReplColorizationPatchTemplate(current.Language);
        return ApplyKeywords(current, patchTemplate);
    }

    private string ApplyKeywords(LanguageConfiguration current, string patchTemplate)
    {
        patchTemplate = patchTemplate.Replace("{LanguageName}", current.LanguageName ?? current.Language);
        patchTemplate = patchTemplate.Replace("{LanguageCode}", current.Language);
        var keywords = new StringBuilder();
        var colorizationKeywords = new StringBuilder();
        var keywordsOverrideCount = 6;
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Abstract, true, "ABSTRACT");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.And, false, "AND");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.As, false, "AS");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Assert, false, "ASSERT");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Base, false, "BASE");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Begin, false, "BEGIN");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Class, false, "CLASS");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Const, true, "CONST");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Default, true, "DEFAULT");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Delegate, true, "DELEGATE");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Do, false, "DO");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Done, false, "DONE");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Elif, true, "ELIF");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Else, false, "ELSE");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.End, false, "END");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Exception, false, "EXCEPTION");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Extern, true, "EXTERN");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.False, false, "FALSE");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Finally, false, "FINALLY");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Fixed, true, "FIXED");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.For, false, "FOR");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Fun, false, "FUN");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Function, false, "FUNCTION");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Global, true, "GLOBAL");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.If, false, "IF");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.In, false, "IN");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Inherit, false, "INHERIT");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Inline, true, "INLINE");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Interface, true, "INTERFACE");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Internal, true, "INTERNAL");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Lazy, false, "LAZY");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Let, false, "LET(false)");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Match, false, "MATCH");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Member, true, "MEMBER");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Mod, false, $"INFIX_STAR_DIV_MOD_OP \"{current.Keywords.Mod}\"");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Module, false, "MODULE");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Mutable, false, "MUTABLE");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Namespace, true, "NAMESPACE");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.New, false, "NEW");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Null, true, "NULL");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Of, false, "OF");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Open, false, "OPEN");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Or, false, "OR");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Override, true, "OVERRIDE");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Private, false, "PRIVATE");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Public, true, "PUBLIC");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Rec, false, "REC");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Return, true, "YIELD(false)");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Sig, false, "SIG");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Static, true, "STATIC");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Struct, false, "STRUCT");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Then, false, "THEN");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.To, false, "TO");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.True, false, "TRUE");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Try, false, "TRY");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Type, false, "TYPE");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Use, true, "LET(true)");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Val, false, "VAL");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Void, true, "VOID");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.When, false, "WHEN");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.While, false, "WHILE");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.With, false, "WITH");
        keywordsOverrideCount += RegisterKeywordOverride(current.Keywords.Yield, true, "YIELD(true)");

        if (keywords.Length > 1)
        {
            keywords = keywords.Remove(keywords.Length - 1, 1);
        }

        patchTemplate = patchTemplate.Replace("{KEYWORDS_OVERRIDE}", keywords.ToString());
        patchTemplate = patchTemplate.Replace("{KEYWORDS_OVERRIDE_COUNT}", keywordsOverrideCount.ToString());
        patchTemplate = patchTemplate.Replace("{COLORIZATION_KEYWORDS_OVERRIDE}", colorizationKeywords.ToString());

        return patchTemplate;


        int RegisterKeywordOverride(string keywordOverride, bool fsharp, string token)
        {
            int i = 0;
            if (!string.IsNullOrEmpty(keywordOverride))
            {
                foreach (var k in keywordOverride.Split(','))
                {
                    if (k.Equals(token, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    if (k == "mod" && token == $"INFIX_STAR_DIV_MOD_OP \"mod\"")
                    {
                        continue;
                    }

                    keywords.AppendLine($@"+            {(fsharp ? "FSHARP" : "ALWAYS")}, ""{k}"", {token}");
                    colorizationKeywords.AppendLine($@"+    ""{k}"",");
                    i++;
                }
            }

            return i;
        }
    }

    private string GetFSharpPatchTemplate(string tfm)
    {
        var resourceKey = $"FSharpKeywordTranslator.Core.patches.fsharp-compiler-{tfm}.txt";
        using var patchStream = typeof(PatchGenerator).Assembly.GetManifestResourceStream(resourceKey) ?? throw new InvalidDataException("The patch for F# compiler is missing from the assembly.");
        using var stringReader = new StreamReader(patchStream);
        return stringReader.ReadToEnd();
    }

    private string GetFableFSharpPatchTemplate(string tfm)
    {
        var resourceKey = $"FSharpKeywordTranslator.Core.patches.fsharp-compiler-{tfm}-simple.patch";
        using var patchStream = typeof(PatchGenerator).Assembly.GetManifestResourceStream(resourceKey) ?? throw new InvalidDataException("The patch for Fable F# compiler is missing from the assembly.");
        using var stringReader = new StreamReader(patchStream);
        return stringReader.ReadToEnd();
    }

    private string GetFableFSharpBuildPatchTemplate(string tfm)
    {
        var resourceKey = $"FSharpKeywordTranslator.Core.patches.fable-fsharp-{tfm}.patch";
        using var patchStream = typeof(PatchGenerator).Assembly.GetManifestResourceStream(resourceKey) ?? throw new InvalidDataException("The patch for Fable F# compiler is missing from the assembly.");
        using var stringReader = new StreamReader(patchStream);
        return stringReader.ReadToEnd();
    }

    private string? GetFableReplPatchTemplate(string tfm, string language)
    {
        var resourceKey = $"FSharpKeywordTranslator.Core.patches.repl-{tfm}-{language}.patch";
        var patchStream = typeof(PatchGenerator).Assembly.GetManifestResourceStream(resourceKey);
        if (patchStream is null)
        {
            return null;
        }

        using (patchStream)
        {
            using var stringReader = new StreamReader(patchStream);
            return stringReader.ReadToEnd();
        }
    }

    private string? GetFableReplColorizationPatchTemplate(string language)
    {
        var resourceKey = $"FSharpKeywordTranslator.Core.patches.repl-colorization.patch";
        var patchStream = typeof(PatchGenerator).Assembly.GetManifestResourceStream(resourceKey);
        if (patchStream is null)
        {
            return null;
        }

        using (patchStream)
        {
            using var stringReader = new StreamReader(patchStream);
            return stringReader.ReadToEnd();
        }
    }
}
