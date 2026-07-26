using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;

namespace FSharpKeywordTranslator.Core;

public class FileTranslator
{
    public void TranslateFSharpFile(FileInfo fileInfo, LanguageConfiguration configuration)
    {
        var content = File.ReadAllText(fileInfo.FullName);
        var translatedContent = content;
        var keywords = configuration.Keywords;
        foreach (var keywordProperty in keywords.GetType().GetProperties())
        {
            var keyword = keywordProperty.GetValue(keywords)?.ToString();
            if (keyword != null)
            {
                var newKeyword = keyword.Split(",")[0];
                translatedContent = Regex.Replace(translatedContent, $@"\b{Regex.Escape(keywordProperty.Name.ToLower())}\b", newKeyword);
            }
        }

        File.WriteAllText(fileInfo.FullName, translatedContent);
    }
}
