# Offline C# compile check for KaramaniReborn: no Unity license, no project lock.
# Compiles Assets (runtime) and Assets/**/Editor (editor) with Unity's bundled Roslyn against the
# Unity 6000.6.0f1 managed assemblies. File lists are rebuilt on every run, so newly added
# scripts are always included.
# It is NOT a substitute for Unity's own import/compile (no asmdef rules, no serialization,
# no shader compile). Gate C still needs BuildTools.Verify. See Docs/BUILD_AND_CAPTURE.md.
#
# Usage:  powershell -NoProfile -ExecutionPolicy Bypass -File Tools/compile_check.ps1
#         ... -ExtraSource path\to\Draft.cs   (compile a draft together with the project)
# Exit code 0 = both assemblies compiled; 1 = compile errors (printed); 2 = toolchain missing.
param(
    [string]$UnityRoot = 'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor',
    [string[]]$ExtraSource = @()
)
$ErrorActionPreference = 'Stop'
$project = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$data = Join-Path $UnityRoot 'Data'
$dotnet = Join-Path $data 'DotNetSdk\dotnet.exe'
$csc = Get-ChildItem -Path (Join-Path $data 'DotNetSdk\sdk') -Filter csc.dll -Recurse -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -like '*Roslyn\bincore\csc.dll' } | Select-Object -First 1
if (-not (Test-Path $dotnet) -or -not $csc) { Write-Host 'COMPILE_CHECK_TOOLCHAIN_MISSING'; exit 2 }

# Per-process work folder: several agents may run this check at the same time.
$work = Join-Path $env:TEMP "karamani_compile_check_$PID"
New-Item -ItemType Directory -Force -Path $work | Out-Null
$runtimeDll = Join-Path $work 'karamani_runtime.dll'
$editorDll = Join-Path $work 'karamani_editor.dll'

$refs = @(Join-Path $data 'NetStandard\ref\2.1.0\netstandard.dll')
$refs += Get-ChildItem -Path (Join-Path $data 'Managed\UnityEngine') -Filter *.dll | ForEach-Object FullName
$ui = Join-Path $project 'Library\ScriptAssemblies\UnityEngine.UI.dll'
if (Test-Path $ui) { $refs += $ui } else { Write-Host 'WARN: Library\ScriptAssemblies\UnityEngine.UI.dll missing; open the project in Unity once.' }

# Messages use Write-Host so they never leak into the function's boolean result.
function Invoke-Csc([string]$name, [string]$out, [string[]]$sources, [string[]]$references) {
    $rsp = Join-Path $work "$name.rsp"
    $lines = @('-target:library', '-langversion:9', '-nostdlib', '-nologo', '-nowarn:CS0436,CS1701,CS1702', "-out:`"$out`"")
    $lines += $references | ForEach-Object { "-r:`"$_`"" }
    $lines += $sources | ForEach-Object { "`"$_`"" }
    Set-Content -Path $rsp -Value $lines -Encoding UTF8
    $output = & $dotnet $csc.FullName "@$rsp" 2>&1
    $code = $LASTEXITCODE
    $errors = @($output | Where-Object { "$_" -match 'error CS' })
    $warnings = @($output | Where-Object { "$_" -match 'warning CS' })
    if ($code -ne 0 -or $errors.Count -gt 0) {
        Write-Host "FAIL: $name ($($sources.Count) files, $($errors.Count) errors)"
        $output | ForEach-Object { Write-Host "  $_" }
        return $false
    }
    Write-Host "PASS: $name ($($sources.Count) files, $($warnings.Count) warnings)"
    $warnings | ForEach-Object { Write-Host "  $_" }
    return $true
}

$all = Get-ChildItem -Path (Join-Path $project 'Assets') -Filter *.cs -Recurse
$runtimeSources = @($all | Where-Object { $_.FullName -notmatch '\\Editor\\' } | ForEach-Object FullName)
$runtimeSources += $ExtraSource | ForEach-Object { [System.IO.Path]::GetFullPath($_) }
$editorSources = @($all | Where-Object { $_.FullName -match '\\Editor\\' } | ForEach-Object FullName)

$ok = [bool](Invoke-Csc 'runtime' $runtimeDll $runtimeSources $refs)
if ($ok -and $editorSources.Count -gt 0) {
    $ok = [bool](Invoke-Csc 'editor' $editorDll $editorSources ($refs + $runtimeDll))
}
Remove-Item -LiteralPath $work -Recurse -Force -ErrorAction SilentlyContinue
if ($ok) { Write-Host 'COMPILE_CHECK_SUCCESS'; exit 0 } else { Write-Host 'COMPILE_CHECK_FAILED'; exit 1 }
