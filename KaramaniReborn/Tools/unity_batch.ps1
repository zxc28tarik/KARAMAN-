# Single safe entry point for Unity batch jobs on KaramaniReborn.
#   powershell -NoProfile -ExecutionPolicy Bypass -File Tools/unity_batch.ps1 -Job Verify
# Jobs (BuildTools / ArtValidation entry points and their success markers):
#   Verify       full movement/combat/presentation checks + chapter/animation captures  (~75-120 s)
#   CaptureLook  only the four chapter captures, for quick look iterations             (~60 s)
#   Build        Windows player into Build/                                             (~60-90 s)
#   ReviewCharacters  single-piece character lineup renders + foot-lock check
# Guarantees: refuses to start if any Unity process already has this project open (Unity allows one
# instance per project); fresh log per run in Logs/batch/; success = exit 0 AND marker in the fresh log;
# exit 198 is reported as a license problem (the user must sign in to Unity Hub; never automate that).
# Exit codes: 0 ok, 1 job failed, 3 project busy, 4 license, 5 timeout, 2 bad arguments / editor missing.
param(
    [Parameter(Mandatory = $true)][ValidateSet('Verify', 'CaptureLook', 'Build', 'ReviewCharacters')][string]$Job,
    [int]$TimeoutSeconds = 900,
    [string]$Unity = 'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe'
)
$ErrorActionPreference = 'Stop'
$project = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if (-not (Test-Path -LiteralPath $Unity)) { Write-Host "UNITY_BATCH_ERROR editor not found: $Unity"; exit 2 }

$jobs = @{
    Verify      = @{ Method = 'KaramaniReborn.Editor.BuildTools.Verify';        Marker = 'KARAMANI_VERIFY_SUCCESS'; Graphics = $true }
    CaptureLook = @{ Method = 'KaramaniReborn.Editor.BuildTools.CaptureLook';   Marker = 'KARAMANI_LOOK_CAPTURED';  Graphics = $true }
    Build       = @{ Method = 'KaramaniReborn.Editor.BuildTools.BuildWindows';  Marker = 'KARAMANI_BUILD_SUCCESS';  Graphics = $false }
    ReviewCharacters = @{ Method = 'KaramaniReborn.Editor.ArtValidation.ReviewCharacters'; Marker = 'KARAMANI_CHARACTER_REVIEW_PASS'; Graphics = $true }
}
$spec = $jobs[$Job]

# One Unity per project: refuse if the editor GUI or another batch job has it open.
$busy = Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" -ErrorAction SilentlyContinue |
    Where-Object { $_.CommandLine -and $_.CommandLine.ToLower().Contains($project.ToLower()) }
if ($busy) {
    Write-Host "UNITY_BATCH_BUSY project is open in Unity (PID $(@($busy | ForEach-Object ProcessId) -join ', ')). Close it or wait; do not kill a process you did not start."
    exit 3
}

$logDir = Join-Path $project 'Logs\batch'
New-Item -ItemType Directory -Force -Path $logDir | Out-Null
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$log = Join-Path $logDir "$Job-$stamp.log"
$started = Get-Date
$unityArgs = @('-batchmode')
if (-not $spec.Graphics) { $unityArgs += '-nographics' }   # camera captures need graphics
$unityArgs += @('-projectPath', "`"$project`"", '-executeMethod', $spec.Method, '-quit', '-logFile', "`"$log`"")
Write-Host "UNITY_BATCH_START $Job $(Get-Date -Format s) log=$log"
$process = Start-Process -FilePath $Unity -ArgumentList $unityArgs -WindowStyle Hidden -PassThru
if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
    Stop-Process -Id $process.Id -Force   # only the process this script started
    Write-Host "UNITY_BATCH_TIMEOUT $Job after $TimeoutSeconds s; killed PID $($process.Id). Log: $log"
    exit 5
}
$code = $process.ExitCode
$elapsed = [math]::Round(((Get-Date) - $started).TotalSeconds, 1)
$text = if (Test-Path $log) { Get-Content -LiteralPath $log -Raw } else { '' }
$errors = @(($text -split "`n") | Where-Object { $_ -match 'error CS\d+|Exception:|InvalidOperationException' } | Select-Object -First 15)
if ($code -eq 198 -or $text -match 'No valid Unity Editor license') {
    Write-Host "UNITY_BATCH_LICENSE exit=$code. The user must sign in to Unity Hub (it may also auto-open). Log: $log"
    exit 4
}
$markerOk = (-not $spec.Marker) -or ($text -match [regex]::Escape($spec.Marker))
if ($code -ne 0 -or -not $markerOk) {
    Write-Host "UNITY_BATCH_FAILED $Job exit=$code marker=$markerOk elapsed=${elapsed}s log=$log"
    $errors | ForEach-Object { Write-Host "  $_" }
    exit 1
}
# Evidence freshness: list Review files written by this run.
$fresh = Get-ChildItem -LiteralPath (Join-Path $project 'Review') -Recurse -File | Where-Object { $_.LastWriteTime -ge $started }
Write-Host "UNITY_BATCH_SUCCESS $Job exit=0 elapsed=${elapsed}s marker=$($spec.Marker) freshReviewFiles=$(@($fresh).Count) log=$log"
$fresh | Select-Object -First 60 | ForEach-Object { Write-Host "  fresh: $($_.FullName.Substring($project.Length + 1))" }
exit 0
