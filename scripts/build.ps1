# AI Workspace Build Script
# Usage: .\scripts\build.ps1

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

Write-Host "=== AI Workspace Build ===" -ForegroundColor Cyan

# Step 1: Build Frontend
Write-Host "`n[1/3] Building React frontend..." -ForegroundColor Yellow
Set-Location "$root\src\AIWorkspace.Web"

# Check if node_modules exists
if (-not (Test-Path "node_modules")) {
    Write-Host "Installing npm dependencies..." -ForegroundColor Gray
    npm install
}

npm run build

if ($LASTEXITCODE -ne 0) {
    Write-Error "Frontend build failed!"
    exit 1
}

# Step 2: Build WPF
Write-Host "`n[2/3] Building WPF application..." -ForegroundColor Yellow
Set-Location "$root"

dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet restore failed!"
    exit 1
}

dotnet build src/AIWorkspace.WPF/AIWorkspace.WPF.csproj -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "WPF build failed!"
    exit 1
}

# Step 3: Publish Single File
Write-Host "`n[3/3] Publishing single-file executable..." -ForegroundColor Yellow
dotnet publish src/AIWorkspace.WPF/AIWorkspace.WPF.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o "$root\publish"

if ($LASTEXITCODE -ne 0) {
    Write-Error "Publish failed!"
    exit 1
}

Write-Host "`n=== Build Complete ===" -ForegroundColor Green
Write-Host "Output: $root\publish\AIWorkspace.WPF.exe" -ForegroundColor Green

# Step 4: Build Installer (optional)
$innoSetup = "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
if (-not (Test-Path $innoSetup)) {
    $innoSetup = "${env:ProgramFiles}\Inno Setup 6\ISCC.exe"
}

if (Test-Path $innoSetup) {
    Write-Host "`n[4/4] Building installer with Inno Setup..." -ForegroundColor Yellow
    & $innoSetup "$root\scripts\setup.iss"
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Installer built successfully." -ForegroundColor Green
    } else {
        Write-Warning "Inno Setup build failed."
    }
} else {
    Write-Host "`n[4/4] Inno Setup not found. Skipping installer build." -ForegroundColor Yellow
    Write-Host "Download from: https://jrsoftware.org/isdl.php" -ForegroundColor Gray
}
