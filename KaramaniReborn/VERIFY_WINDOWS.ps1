$ErrorActionPreference = 'Stop'
$karamaniPlayer = Join-Path $PSScriptRoot 'Build\KaramaniReborn.exe'
if (-not (Test-Path -LiteralPath $karamaniPlayer)) { throw 'Önce Windows sürümünü derle.' }
$karamaniReview = Join-Path $PSScriptRoot 'Review'
New-Item -ItemType Directory -Path $karamaniReview -Force | Out-Null
$karamaniPlayerLog = Join-Path $karamaniReview 'player.log'
$karamaniAuditArgs = @('-batchmode', '-screen-fullscreen', '0', '-screen-width', '1280', '-screen-height', '720', '--karamani-verify', '--karamani-output', ('"{0}"' -f $karamaniReview), '-logFile', ('"{0}"' -f $karamaniPlayerLog))
$karamaniAudit = Start-Process -FilePath $karamaniPlayer -ArgumentList $karamaniAuditArgs -WindowStyle Hidden -PassThru
if (-not $karamaniAudit.WaitForExit(120000)) {
    Stop-Process -Id $karamaniAudit.Id
    throw 'Sadece bu betiğin başlattığı doğrulama oyuncusu zaman aşımında kapatıldı.'
}
if ($karamaniAudit.ExitCode -ne 0 -or -not (Select-String -LiteralPath $karamaniPlayerLog -SimpleMatch 'KARAMANI_PLAYER_VERIFY_SUCCESS' -Quiet)) { throw "EXE doğrulaması başarısız. Günlük: $karamaniPlayerLog" }
Get-Content -LiteralPath (Join-Path $karamaniReview 'player-verification.txt')
