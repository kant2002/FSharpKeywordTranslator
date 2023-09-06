# FSharp Keyword Translation

Utility web application to support translation of F# keywords to different languages. Application show code snippets, so native speakers can validate that code looks legit to them.

```
.\FSharp.ps1 -FSharpRepo C:\dotnet\fsharp -Language uk -OutputStorage C:\fsharp-ua-lang\mulilang-fsharp
```

Only Ukrainian has translation for REPL UI. If other translations would be added, change this line and `fable.ps1` to enable applying patch.
```
.\fable.ps1 -FSharpRepo C:\dotnet\fsharp -Language uk -OutputStorage C:\fsharp-ua-lang\mulilang-fsharp -FableRepo C:\fable\Fable -FableReplRepo C:\fable\repl\
```
