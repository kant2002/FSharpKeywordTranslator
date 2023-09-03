using System.Text;

namespace FSharpKeywordTranslator.Core;

public class PatchGenerator
{
    public string GenerateFSharpPatch(LanguageConfiguration current)
    {
        var fileStream = GetFileStream();
        fileStream = fileStream.Replace("{LanguageName}", current.LanguageName ?? current.Language);
        var keywords = new StringBuilder();
        var keywordsOverrideCount = 6;
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Abstract, true, "ABSTRACT");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.And, false, "AND");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.As, false, "AS");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Assert, false, "ASSERT");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Base, false, "BASE");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Begin, false, "BEGIN");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Class, false, "CLASS");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Const, true, "CONST");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Default, true, "DEFAULT");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Delegate, true, "DELEGATE");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Do, false, "DO");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Done, false, "DONE");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Elif, true, "ELIF");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Else, false, "ELSE");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.End, false, "END");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Exception, false, "EXCEPTION");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Extern, true, "EXTERN");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.False, false, "FALSE");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Finally, false, "FINALLY");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Fixed, true, "FIXED");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.For, false, "FOR");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Fun, false, "FUN");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Function, false, "FUNCTION");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Global, true, "GLOBAL");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.If, false, "IF");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.In, false, "IN");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Inherit, false, "INHERIT");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Inline, true, "INLINE");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Interface, true, "INTERFACE");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Internal, true, "INTERNAL");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Lazy, false, "LAZY");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Let, false, "LET(false)");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Match, false, "MATCH");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Member, true, "MEMBER");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Mod, false, $"INFIX_STAR_DIV_MOD_OP \"{current.Keywords.Mod}\"");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Module, false, "MODULE");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Mutable, false, "MUTABLE");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Namespace, true, "NAMESPACE");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.New, false, "NEW");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Null, true, "NULL");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Of, false, "OF");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Open, false, "OPEN");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Or, false, "OR");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Override, true, "OVERRIDE");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Private, false, "PRIVATE");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Public, true, "PUBLIC");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Rec, false, "REC");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Return, true, "YIELD(false)");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Sig, false, "SIG");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Static, true, "STATIC");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Struct, false, "STRUCT");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Then, false, "THEN");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.To, false, "TO");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.True, false, "TRUE");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Try, false, "TRY");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Type, false, "TYPE");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Use, true, "LET(true)");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Val, false, "VAL");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Void, true, "VOID");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.When, false, "WHEN");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.While, false, "WHILE");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.With, false, "WITH");
        keywordsOverrideCount += RegisterKeywordOverride(keywords, current.Keywords.Yield, true, "YIELD(true)");

        if (keywords.Length > 1)
        {
            keywords = keywords.Remove(keywords.Length - 1, 1);
        }

        fileStream = fileStream.Replace("{KEYWORDS_OVERRIDE}", keywords.ToString());
        fileStream = fileStream.Replace("{KEYWORDS_OVERRIDE_COUNT}", keywordsOverrideCount.ToString());

        return fileStream;
    }

    int RegisterKeywordOverride(StringBuilder keywords, string keywordOverride, bool fsharp, string token)
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
                i++;
            }
        }

        return i;
    }

    private string GetFileStream()
    {
        using var patchStream = typeof(PatchGenerator).Assembly.GetManifestResourceStream("FSharpKeywordTranslator.Core.patches.fsharp-compiler-net8.txt") ?? throw new InvalidDataException("The patch for F# compiler is missing from the assembly.");
        using var stringReader = new StreamReader(patchStream);
        return stringReader.ReadToEnd();
    }
}
