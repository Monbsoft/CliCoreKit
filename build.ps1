#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build script for CliCoreKit NuGet packages

.DESCRIPTION
    Builds and packages CliCoreKit libraries with version management

.PARAMETER Version
    The version number to use (e.g., "1.0.0", "1.2.3-beta")

.PARAMETER Configuration
    Build configuration (Debug or Release). Default is Release

.PARAMETER Output
    Output directory for NuGet packages. Default is ./artifacts

.PARAMETER Clean
    Clean before building

.EXAMPLE
    .\build.ps1 -Version "1.0.0"
    .\build.ps1 -Version "1.2.0-beta" -Configuration Debug
    .\build.ps1 -Version "2.0.0" -Clean
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$Version = "1.0.0",

    [Parameter(Mandatory=$false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [Parameter(Mandatory=$false)]
    [string]$Output = "./artifacts",

    [Parameter(Mandatory=$false)]
    [switch]$Clean
)

$ErrorActionPreference = "Stop"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  CliCoreKit Build Script" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Version: $Version" -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Green
Write-Host "Output: $Output" -ForegroundColor Green
Write-Host ""

# Clean if requested
if ($Clean) {
    Write-Host "Cleaning..." -ForegroundColor Yellow
    dotnet clean -c $Configuration
    if (Test-Path $Output) {
        Remove-Item $Output -Recurse -Force
    }
}

# Create output directory
if (-not (Test-Path $Output)) {
    New-Item -ItemType Directory -Path $Output | Out-Null
}

# Restore dependencies
Write-Host "Restoring dependencies..." -ForegroundColor Yellow
dotnet restore

# Build solution
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build -c $Configuration --no-restore /p:Version=$Version

# Run tests
Write-Host "Running tests..." -ForegroundColor Yellow
dotnet test -c $Configuration --no-build --verbosity normal

if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed!" -ForegroundColor Red
    exit 1
}

# Pack Core library
Write-Host "Packing Monbsoft.CliCoreKit.Core..." -ForegroundColor Yellow
dotnet pack src/CliCoreKit.Core/CliCoreKit.Core.csproj `
    -c $Configuration `
    --no-build `
    -o $Output `
    /p:Version=$Version `
    /p:PackageVersion=$Version

# Pack Hosting library
Write-Host "Packing Monbsoft.CliCoreKit.Hosting..." -ForegroundColor Yellow
dotnet pack src/CliCoreKit.Hosting/CliCoreKit.Hosting.csproj `
    -c $Configuration `
    --no-build `
    -o $Output `
    /p:Version=$Version `
    /p:PackageVersion=$Version

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Build Completed Successfully!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Packages created in: $Output" -ForegroundColor Green
Write-Host ""

Get-ChildItem $Output -Filter "*.nupkg" | ForEach-Object {
    Write-Host "  - $($_.Name)" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "To publish to NuGet.org:" -ForegroundColor Yellow
Write-Host "  dotnet nuget push $Output\*.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json" -ForegroundColor Gray
Write-Host ""
