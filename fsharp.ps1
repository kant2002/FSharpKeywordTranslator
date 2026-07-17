param (
    [string]$FSharpRepo = $(throw "-FSharpRepo is required."),
    [string]$OutputStorage = $(throw "-OutputStorage is required."),
    [string]$Language = $(throw "-Language is required.")
)

$ErrorActionPreference = 'Stop'
$PrepareRepo = $true
if ($PrepareRepo) {
    git -C "$FSharpRepo" checkout -- .
    git -C "$FSharpRepo" checkout main
    Remove-Item –path  "$FSharpRepo\artifacts\" –Recurse -Force
    dotnet run --project FSharpKeywordTranslator.Cli --  fsharp --lang $Language | git -C "$FSharpRepo" apply
}
try {
    pushd $FSharpRepo
    $PackCustomFSharp = $true
    if ($PackCustomFSharp) {
        .\Build.cmd -pack -ci /p:OfficialBuild=true /p:PublishWindowsPdb=false /p:VisualStudioDropName=dummy -c Release
    }

    mkdir "$OutputStorage\$Language\artifacts\bin\fsc\Release" -Force
    mkdir "$OutputStorage\$Language\artifacts\bin\fsi\Release" -Force
    mkdir "$OutputStorage\$Language\artifacts\VSSetup\Release" -Force
    Copy-Item "$FSharpRepo\artifacts\bin\fsc\Release\*" -Destination "$OutputStorage\$Language\artifacts\bin\fsc\Release" -Recurse
    Copy-Item "$FSharpRepo\artifacts\bin\fsi\Release\*" -Destination "$OutputStorage\$Language\artifacts\bin\fsi\Release" -Recurse
    Copy-Item "$FSharpRepo\artifacts\VSSetup\Release\*" -Destination "$OutputStorage\$Language\artifacts\VSSetup\Release" -Recurse
} finally {
    popd
}
