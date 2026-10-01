# Build script for Mahyar Free VPN
# Usage: .\build.ps1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Mahyar Free VPN - Build Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if sing-box.exe exists
$singBoxPath = "src\MahyarFree\bin\sing-box.exe"
if (-not (Test-Path $singBoxPath)) {
    Write-Host "⚠️  WARNING: sing-box.exe not found at: $singBoxPath" -ForegroundColor Yellow
    Write-Host "Please download it from: https://github.com/SagerNet/sing-box/releases" -ForegroundColor Yellow
    Write-Host ""
    $continue = Read-Host "Continue anyway? (y/n)"
    if ($continue -ne "y") { exit }
}

# Restore packages
Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Cyan
dotnet restore src\MahyarFree.sln
if ($LASTEXITCODE -ne 0) { Write-Host "❌ Restore failed" -ForegroundColor Red; exit 1 }

# Build
Write-Host "🔨 Building project..." -ForegroundColor Cyan
dotnet build src\MahyarFree.sln -c Release
if ($LASTEXITCODE -ne 0) { Write-Host "❌ Build failed" -ForegroundColor Red; exit 1 }

# Publish single-file
Write-Host "📦 Publishing single-file EXE..." -ForegroundColor Cyan
$outputDir = "src\MahyarFree\bin\Release\net6.0-windows\win-x64\publish"
dotnet publish src\MahyarFree\MahyarFree.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true

if ($LASTEXITCODE -ne 0) { Write-Host "❌ Publish failed" -ForegroundColor Red; exit 1 }

# Copy sing-box to output
if (Test-Path $singBoxPath) {
    Copy-Item $singBoxPath -Destination "$outputDir\sing-box.exe" -Force
    Write-Host "✅ sing-box.exe copied to output" -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  ✅ Build complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Output: $outputDir\MahyarFree.exe" -ForegroundColor Cyan
Write-Host ""

# Open output folder
$open = Read-Host "Open output folder? (y/n)"
if ($open -eq "y") { Start-Process explorer.exe $outputDir }
