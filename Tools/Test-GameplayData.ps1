$ErrorActionPreference = 'Stop'
$unityData = 'C:\Program Files\Unity\Hub\Editor\2021.3.16f1\Editor\Data'
$output = Join-Path $PSScriptRoot '../Temp/Validation/DataTests.exe'
New-Item -ItemType Directory -Force (Split-Path $output) | Out-Null
$sources = @("$PSScriptRoot/GameplayDataTests.cs", "$PSScriptRoot/../Assets/scripts/LevelLayout.cs")
foreach ($name in @('ScoreService', 'ProgressionService')) {
    $path = "$PSScriptRoot/../Assets/scripts/$name.cs"
    if (Test-Path $path) { $sources += $path }
}
& "$unityData/MonoBleedingEdge/bin/mono.exe" "$unityData/MonoBleedingEdge/lib/mono/4.5/csc.exe" /nologo /target:exe "/out:$output" @sources
if ($LASTEXITCODE -ne 0) { throw 'Data test compilation failed' }
& "$unityData/MonoBleedingEdge/bin/mono.exe" $output "$PSScriptRoot/../Assets/Levels"
if ($LASTEXITCODE -ne 0) { throw 'Data tests failed' }
