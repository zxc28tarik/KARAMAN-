$ErrorActionPreference = 'Stop'
$karamaniUnity = 'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe'
if (-not (Test-Path -LiteralPath $karamaniUnity)) { throw 'Unity 6000.6.0f1 bulunamadı. BUILD_WINDOWS.ps1 içindeki editör yolunu güncelle.' }
$karamaniBuildLog = Join-Path $PSScriptRoot 'build.log'
$karamaniArguments = @('-batchmode', '-nographics', '-projectPath', ('"{0}"' -f $PSScriptRoot), '-executeMethod', 'KaramaniReborn.Editor.BuildTools.BuildWindows', '-quit', '-logFile', ('"{0}"' -f $karamaniBuildLog))
$karamaniBuildProcess = Start-Process -FilePath $karamaniUnity -ArgumentList $karamaniArguments -WindowStyle Hidden -PassThru
$karamaniBuildProcess.WaitForExit()
if ($karamaniBuildProcess.ExitCode -ne 0) { throw "Derleme başarısız. Günlük: $karamaniBuildLog" }
Write-Output (Join-Path $PSScriptRoot 'Build\KaramaniReborn.exe')
