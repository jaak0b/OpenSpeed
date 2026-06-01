#Requires -Version 5.1
<#
.SYNOPSIS
    Builds OpenSpeed and packages it into release ZIP archives.

.PARAMETER Version
    Release version string, e.g. "1.0" or "1.1.0". Defaults to "1.0".

.EXAMPLE
    .\build_release.ps1
    .\build_release.ps1 -Version "1.1"
#>
param(
    [string]$Version = "1.0"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot    = Resolve-Path "$PSScriptRoot\.."
$releaseDir  = Join-Path $repoRoot "Release"
$stagingApp  = Join-Path $releaseDir "_staging\OpenSpeed-v$Version"
$stagingArduino = Join-Path $releaseDir "_staging\OpenSpeed-Arduino-v$Version"
$appZip      = Join-Path $releaseDir "OpenSpeed-v$Version.zip"
$arduinoZip  = Join-Path $releaseDir "OpenSpeed-Arduino-v$Version.zip"
$uiCsproj    = Join-Path $repoRoot "OpenSpeed Desktop\OpenSpeed.UI\OpenSpeed.UI.csproj"
$sketchDir   = Join-Path $repoRoot "Arduino Sketch\OpenSpeed"

Write-Host ""
Write-Host "=== OpenSpeed Release Build v$Version ===" -ForegroundColor Cyan
Write-Host ""

# ── 0. Kill running instance to release file locks ───────────────────────────
try { Get-Process -Name "OpenSpeed.UI" -ErrorAction Stop | Stop-Process -Force; Write-Host "Stopped running OpenSpeed.UI" } catch {}

# ── 1. Clean staging & output dirs ───────────────────────────────────────────
if (Test-Path $releaseDir\_staging) { Remove-Item $releaseDir\_staging -Recurse -Force }
New-Item -ItemType Directory -Path $stagingApp     -Force | Out-Null
New-Item -ItemType Directory -Path $stagingArduino -Force | Out-Null
foreach ($zip in @($appZip, $arduinoZip)) {
    if (Test-Path $zip) { Remove-Item $zip -Force }
}
New-Item -ItemType Directory -Path $releaseDir -Force | Out-Null

# ── 2. Publish desktop app (Release, framework-dependent, win-x64) ────────────
Write-Host "Publishing desktop app..." -ForegroundColor Yellow
dotnet publish $uiCsproj `
    --configuration Release `
    --runtime win-x64 `
    --self-contained false `
    --output $stagingApp `
    --nologo `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    | Where-Object { $_ -notmatch "^\s*$" }

if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed." }

# Remove any leftover .pdb files just in case
Get-ChildItem $stagingApp -Filter "*.pdb" | Remove-Item -Force

Write-Host "App published to: $stagingApp" -ForegroundColor Green

# ── 3. Copy Arduino sketch ────────────────────────────────────────────────────
Write-Host "Copying Arduino sketch..." -ForegroundColor Yellow
Copy-Item -Path $sketchDir -Destination $stagingArduino -Recurse -Force
Write-Host "Sketch copied to: $stagingArduino" -ForegroundColor Green

# ── 4. Create ZIP archives ────────────────────────────────────────────────────
Write-Host "Creating ZIP archives..." -ForegroundColor Yellow

Compress-Archive -Path "$stagingApp\*"      -DestinationPath $appZip     -Force
Compress-Archive -Path "$stagingArduino\*"  -DestinationPath $arduinoZip -Force

# ── 5. Clean up staging ───────────────────────────────────────────────────────
Remove-Item "$releaseDir\_staging" -Recurse -Force

# ── 6. Summary ────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "=== Done ===" -ForegroundColor Cyan
Get-ChildItem $releaseDir | ForEach-Object {
    $size = [math]::Round($_.Length / 1MB, 2)
    Write-Host ("  {0,-45} {1,6} MB" -f $_.Name, $size) -ForegroundColor White
}
Write-Host ""
