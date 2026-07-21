param (
    [string]$FSharpRepo = $(throw "-FSharpRepo is required."),
    [string]$FableRepo = $(throw "-FableRepo is required."),
    [string]$FableReplRepo = $(throw "-FableReplRepo is required."),
    [string]$OutputStorage = $(throw "-OutputStorage is required."),
    [string]$Language = $(throw "-Language is required."),
    [switch]$DoNotBuildFSharp = $false,
    [switch]$DoNotBuildFable = $false,
    [switch]$DoNotBuildRepl = $false,
    [switch]$DoNotBuildColorization = $false,
    [string]$Tfm = "net11.0"
)

$ErrorActionPreference = 'Stop'
$TfmBranches = @{
    "net10.0" = "ncave/service_slim_2025-11-21"
    "net11.0" = "ncave/service_slim"
}

if (-not $DoNotBuildFSharp)
{
    git -C "$FSharpRepo" checkout -- .
    git -C "$FSharpRepo" checkout --detach $TfmBranches[$Tfm]
    if (Test-Path "$FSharpRepo\artifacts\") {
        Remove-Item –path  "$FSharpRepo\artifacts\" –Recurse -Force
    }
    Write-Host "Applying F# patch"
    Write-Host "dotnet run --project FSharpKeywordTranslator.Cli --  fable --tfm $Tfm --lang $Language"
    dotnet run --project FSharpKeywordTranslator.Cli --  fable --tfm $Tfm --lang $Language | git -C "$FSharpRepo" apply
    Write-Host "Applying F# build patch"
    Write-Host "dotnet run --project FSharpKeywordTranslator.Cli --  fable-build --tfm $Tfm --lang $Language"
    dotnet run --project FSharpKeywordTranslator.Cli --  fable-build --tfm $Tfm --lang $Language | git -C "$FSharpRepo" apply
    try {
        pushd $FSharpRepo
        bash fcs/build.sh
        mkdir "$OutputStorage\$Language\fable" -Force
        Copy-Item "$FSharpRepo\artifacts/bin/FSharp.Compiler.Service/Release/netstandard2.0/FSharp.Compiler.Service.*" -Destination "$OutputStorage\$Language\fable\" -Recurse
        Copy-Item "$FSharpRepo\artifacts/bin/FSharp.Compiler.Service/Release/netstandard2.0/FSharp.Core.*" -Destination "$OutputStorage\$Language\fable\" -Recurse
    } finally {
        popd
    }
}

if (-not $DoNotBuildFable)
{
    Write-Host "Checkout out fable repository"
    git -C "$FableRepo" checkout -- .
    git -C "$FableRepo" checkout main
    Write-Host "Applying Fable patch"
    Write-Host "dotnet run --project FSharpKeywordTranslator.Cli --  fable --tfm $Tfm --lang $Language"
    dotnet run --project FSharpKeywordTranslator.Cli --  fable --tfm $Tfm --lang $Language | git -C "$FableRepo\src\fcs-fable" apply --directory=src/fcs-fable/ --ignore-space-change
    Write-Host "Copying built F# compiler service"
    Copy-Item "$OutputStorage\$Language\fable\*" -Destination "$FableRepo\lib\fcs\" -Recurse
    try {
        pushd $FableRepo
        Write-Host "Start Fable build"
        #dotnet fsi build.fsx standalone
        ./build.bat standalone
    } finally {
        popd
    }
}


if (-not $DoNotBuildRepl)
{
    $env:LOCAL_PKG=1
    git -C "$FableReplRepo" checkout -- .
    git -C "$FableReplRepo" checkout main
    Write-Host "Applying UI patch"
    Write-Host "dotnet run --project FSharpKeywordTranslator.Cli --  repl --tfm $Tfm --lang $Language"
    dotnet run --project FSharpKeywordTranslator.Cli -- repl --tfm $Tfm --lang $Language | git -C "$FableReplRepo" apply

    try {
        pushd $FableReplRepo
        if (Test-Path global.json) {
            Write-Host "Removing global.json"
            Remove-Item global.json
        }
        Write-Host "Restoring local tools"
        dotnet tool restore
        Write-Host "Building REPL"
        dotnet fsi build.fsx -p BuildApp --local
    } finally {
        popd
    }
}

if (-not $DoNotBuildColorization)
{
    try {
        if (Test-Path "$FableReplRepo/node_modules/monaco-editor/esm/vs/basic-languages/fsharp/_fsharp.js")
        {
            Write-Host "Restore saved _fsharp.js"
            Copy-Item "$FableReplRepo/node_modules/monaco-editor/esm/vs/basic-languages/fsharp/_fsharp.js" "$FableReplRepo/node_modules/monaco-editor/esm/vs/basic-languages/fsharp/fsharp.js" -Force
        }
        else
        {
            Write-Host "Save _fsharp.js"
            Copy-Item "$FableReplRepo/node_modules/monaco-editor/esm/vs/basic-languages/fsharp/fsharp.js" "$FableReplRepo/node_modules/monaco-editor/esm/vs/basic-languages/fsharp/_fsharp.js" -Force
        }
        git  -C "$FableReplRepo" checkout -- src/App/vite.config.ts
        Write-Host "Applying Repl colorization patch"
        dotnet run --project FSharpKeywordTranslator.Cli -- repl-colorization --lang $Language | git -C "$FableReplRepo" apply
        pushd $FableReplRepo/src/App
        npx vite build
    } finally {
        popd
    }
}
