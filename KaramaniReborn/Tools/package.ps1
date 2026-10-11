param([string]$Destination)
$ErrorActionPreference = 'Stop'
$karamaniProject = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if (-not $Destination) { $Destination = Join-Path (Split-Path $karamaniProject -Parent) 'KaramaniReborn-SOTN-Unity-ve-Windows.zip' }
$karamaniDestination = [System.IO.Path]::GetFullPath($Destination)
if (Test-Path -LiteralPath $karamaniDestination) { throw "Mevcut paket korunuyor. Yeni bir hedef dosya adı belirt: $karamaniDestination" }
if (-not (Test-Path -LiteralPath (Join-Path $karamaniProject 'Build\KaramaniReborn.exe'))) { throw 'Önce Windows sürümünü derle.' }
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$karamaniFiles = @()
foreach ($karamaniFolder in @('Assets', 'Packages', 'ProjectSettings', 'Build', 'Docs', 'Review', 'Tools')) {
    $karamaniFiles += Get-ChildItem -LiteralPath (Join-Path $karamaniProject $karamaniFolder) -File -Recurse -Force
}
foreach ($karamaniName in @('README.md', 'BUILD_WINDOWS.ps1', 'VERIFY_WINDOWS.ps1', '.gitignore', 'build.log', 'verify.log')) {
    $karamaniFiles += Get-Item -LiteralPath (Join-Path $karamaniProject $karamaniName)
}
$karamaniStream = [System.IO.File]::Open($karamaniDestination, [System.IO.FileMode]::CreateNew)
$karamaniArchive = [System.IO.Compression.ZipArchive]::new($karamaniStream, [System.IO.Compression.ZipArchiveMode]::Create, $false)
$karamaniCount = 0
try {
    foreach ($karamaniFile in ($karamaniFiles | Sort-Object FullName -Unique)) {
        if (-not $karamaniFile.FullName.StartsWith($karamaniProject + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Paket girdisi proje klasörü dışında.' }
        $karamaniRelative = $karamaniFile.FullName.Substring($karamaniProject.Length + 1).Replace('\', '/')
        # Obsolete pre-validation close-up is kept on disk, but is not a current review.
        if ($karamaniRelative -eq 'Review/rig-closeup.png') { continue }
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($karamaniArchive, $karamaniFile.FullName, $karamaniRelative, [System.IO.Compression.CompressionLevel]::Optimal) | Out-Null
        $karamaniCount++
    }
}
finally { $karamaniArchive.Dispose(); $karamaniStream.Dispose() }
Write-Output "Paket: $karamaniDestination"
Write-Output "Dosya: $karamaniCount; bayt: $((Get-Item -LiteralPath $karamaniDestination).Length)"
