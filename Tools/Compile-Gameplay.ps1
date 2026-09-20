param([string]$Milestone = 'Current')
$ErrorActionPreference = 'Stop'
$unityData = 'C:\Program Files\Unity\Hub\Editor\2021.3.16f1\Editor\Data'
$outDir = Join-Path $PSScriptRoot "../Temp/Validation/$Milestone"
New-Item -ItemType Directory -Force $outDir | Out-Null
$compileArgs = @("$unityData\MonoBleedingEdge\lib\mono\4.5\csc.exe", '/nologo', '/target:library', "/out:$outDir/Gameplay.dll")
Get-ChildItem "$unityData/Managed/UnityEngine/*.dll" | ForEach-Object { $compileArgs += "/reference:$($_.FullName)" }
$compileArgs += "/reference:$PSScriptRoot/../Library/ScriptAssemblies/UnityEngine.UI.dll"
$compileArgs += "/reference:$unityData/MonoBleedingEdge/lib/mono/unityjit-win32/Facades/netstandard.dll"
$compileArgs += @(Get-ChildItem "$PSScriptRoot/../Assets/scripts" -Recurse -Filter *.cs | Select-Object -ExpandProperty FullName)
& "$unityData/MonoBleedingEdge/bin/mono.exe" @compileArgs
if ($LASTEXITCODE -ne 0) { throw 'Gameplay compilation failed' }
Write-Output "${Milestone}: gameplay C# compilation passed (not a Unity import or Play Mode test)."

