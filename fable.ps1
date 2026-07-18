param (
    [string]$FSharpRepo = $(throw "-FSharpRepo is required."),
    [string]$FableRepo = $(throw "-FableRepo is required."),
    [string]$FableReplRepo = $(throw "-FableReplRepo is required."),
    [string]$OutputStorage = $(throw "-OutputStorage is required."),
    [string]$Language = $(throw "-Language is required."),
    [switch]$DoNotBuildFSharp = $false,
    [switch]$DoNotBuildFable = $false,
    [switch]$DoNotBuildRepl = $false,
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
    dotnet run --project FSharpKeywordTranslator.Cli --  fable --tfm $Tfm --lang $Language | git -C "$FSharpRepo" apply
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
    Write-Host "Applying patch"
    #dotnet run --project FSharpKeywordTranslator.Cli --  fable --tfm $Tfm --lang $Language | git -C "$FableRepo\src\fcs-fable" apply --directory=src/fcs-fable/ --ignore-space-change
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
    git -C "$FableReplRepo" checkout master
    Write-Host "Applying UI patch"
    dotnet run --project FSharpKeywordTranslator.Cli -- repl --lang $Language | git -C "$FableReplRepo" apply

    try {
        pushd $FableReplRepo
        dotnet tool restore
        dotnet fake build -t All
    } finally {
        popd
    }
}
