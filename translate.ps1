param (
    [string]$OutputStorage = $(throw "-OutputStorage is required."),
    [string]$Language = $(throw "-Language is required.")
)

$files = @(
    "tour/primitives.fs",
    "tour/functions.fs",
    "tour/collections.fs",
    "tour/records.fs",
    "tour/unions.fs",
    "tour/units.fs",
    "tour/classes.fs",

    "visual/basic-canvas.fs",
    "visual/mandelbrot.fs",
    "visual/raytracer.fs",
    "visual/hokusai.fs",
    "visual/color-fountain.fs",
    "visual/fractal.fs",

    "games/undertone.fs",
    "games/ants.fs",
    "games/mario.fs",
    "games/ozmo.fs",
    "games/pacman.fs",
    
    "ui/spreadsheet.fs",
    "ui/webcomponent.fs"
)

foreach ($file in $files) {
    Write-Host "Translating $file"
    dotnet run --project FSharpKeywordTranslator.Cli -- translate --lang $Language --file "$OutputStorage\fable-repl-$Language\samples\$file"
}