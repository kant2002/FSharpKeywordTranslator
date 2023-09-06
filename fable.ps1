param (
    [string]$FSharpRepo = $(throw "-FSharpRepo is required."),
    [string]$FableRepo = $(throw "-FableRepo is required."),
    [string]$FableReplRepo = $(throw "-FableReplRepo is required."),
    [string]$OutputStorage = $(throw "-OutputStorage is required."),
    [string]$Language = $(throw "-Language is required."),
    [switch]$DoNotBuildFSharp = $false,
    [switch]$DoNotBuildFable = $false,
    [switch]$DoNotBuildRepl = $false
)

if (-not $DoNotBuildFSharp)
{
    git -C "$FSharpRepo" checkout -- .
    git -C "$FSharpRepo" checkout --detach ncave/service_slim
    Remove-Item –path  "$FSharpRepo\artifacts\" –Recurse
    dotnet run --project FSharpKeywordTranslator.Cli --  fable --lang $Language | git -C "$FSharpRepo" apply
    try {
        pushd $FSharpRepo
        bash fcs/build.sh
        mkdir "$OutputStorage\$Language\fable"
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
    dotnet run --project FSharpKeywordTranslator.Cli --  fable --lang $Language | git -C "$FableRepo\src\fcs-fable" apply --directory=src/fcs-fable/ --ignore-space-change
    Write-Host "Copying built F# compiler service"
    Copy-Item "$OutputStorage\$Language\fable\*" -Destination "$FableRepo\lib\fcs\" -Recurse
    try {
        pushd $FableRepo
        dotnet fsi build.fsx standalone
    } finally {
        popd
    }
}


if (-not $DoNotBuildRepl)
{
    $env:LOCAL_PKG=1
    try {
        pushd $FableReplRepo
        dotnet tool restore
        dotnet fake build -t All
    } finally {
        popd
    }
}
