#Requires -Version 5.1
<#
.SYNOPSIS
    Installs the MEP QC Checker Revit plugin for all detected Revit versions.
.DESCRIPTION
    Detects installed Revit versions via registry and copies the correct build
    (net48 for 2020-2024, net8 for 2025-2026) to each user's Revit Addins folder.
    No admin rights required.
#>

param(
    [string]$BuildRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"

Write-Host "`n=== MEP QC Checker Installer ===" -ForegroundColor Cyan
Write-Host ""

# Build output paths
$Net48Output = Join-Path $BuildRoot "src\MEPQCChecker.Revit2024\bin\Release\net48"
$Net8Output  = Join-Path $BuildRoot "src\MEPQCChecker.Revit2025\bin\Release\net8.0-windows"
$AddinTemplate = Join-Path $BuildRoot "installer\MEPQCChecker.addin"

# Detect installed Revit versions
$revitVersions = @()
$regPaths = @(
    "HKLM:\SOFTWARE\Autodesk\Revit",
    "HKLM:\SOFTWARE\WOW6432Node\Autodesk\Revit"
)

foreach ($regPath in $regPaths) {
    if (Test-Path $regPath) {
        Get-ChildItem $regPath -ErrorAction SilentlyContinue | ForEach-Object {
            $name = $_.PSChildName
            if ($name -match "(\d{4})") {
                $year = [int]$Matches[1]
                if ($year -ge 2020 -and $year -le 2026 -and $revitVersions -notcontains $year) {
                    $revitVersions += $year
                }
            }
        }
    }
}

if ($revitVersions.Count -eq 0) {
    Write-Host "No supported Revit versions (2020-2026) found in registry." -ForegroundColor Yellow
    Write-Host "Checking standard Addins folders as fallback..." -ForegroundColor Yellow

    # Fallback: check if Addins folders exist
    $addinsBase = Join-Path $env:APPDATA "Autodesk\Revit\Addins"
    if (Test-Path $addinsBase) {
        Get-ChildItem $addinsBase -Directory | ForEach-Object {
            if ($_.Name -match "^(\d{4})$") {
                $year = [int]$_.Name
                if ($year -ge 2020 -and $year -le 2026) {
                    $revitVersions += $year
                }
            }
        }
    }
}

if ($revitVersions.Count -eq 0) {
    Write-Host "No Revit installations found. Please install Revit first." -ForegroundColor Red
    exit 1
}

$revitVersions = $revitVersions | Sort-Object
Write-Host "Detected Revit versions: $($revitVersions -join ', ')" -ForegroundColor Green
Write-Host ""

$installed = 0

foreach ($year in $revitVersions) {
    Write-Host "Installing for Revit $year..." -ForegroundColor White

    # Determine which build to use
    if ($year -le 2024) {
        $sourceDir = $Net48Output
        $buildName = "net48"
    } else {
        $sourceDir = $Net8Output
        $buildName = "net8.0-windows"
    }

    if (-not (Test-Path $sourceDir)) {
        Write-Host "  WARNING: Build output not found at $sourceDir" -ForegroundColor Yellow
        Write-Host "  Run 'dotnet build -c Release' first." -ForegroundColor Yellow
        continue
    }

    # Target paths
    $addinsDir = Join-Path $env:APPDATA "Autodesk\Revit\Addins\$year"
    $pluginDir = Join-Path $addinsDir "MEPQCChecker"
    $addinFile = Join-Path $addinsDir "MEPQCChecker.addin"

    # Create directories
    if (-not (Test-Path $pluginDir)) {
        New-Item -ItemType Directory -Path $pluginDir -Force | Out-Null
    }

    # Copy DLLs and config
    $filesToCopy = @(
        "MEPQCChecker.Revit.dll",
        "MEPQCChecker.Core.dll",
        "config.json"
    )

    foreach ($file in $filesToCopy) {
        $src = Join-Path $sourceDir $file
        if (Test-Path $src) {
            Copy-Item $src -Destination $pluginDir -Force
            Write-Host "  Copied $file" -ForegroundColor DarkGray
        }
    }

    # Copy System.Text.Json and dependencies (for net48)
    if ($year -le 2024) {
        Get-ChildItem $sourceDir -Filter "System.*.dll" | ForEach-Object {
            Copy-Item $_.FullName -Destination $pluginDir -Force
            Write-Host "  Copied $($_.Name)" -ForegroundColor DarkGray
        }
        Get-ChildItem $sourceDir -Filter "Microsoft.Bcl.*.dll" -ErrorAction SilentlyContinue | ForEach-Object {
            Copy-Item $_.FullName -Destination $pluginDir -Force
        }
    }

    # Create .addin manifest (update assembly path)
    if (Test-Path $AddinTemplate) {
        $addinContent = Get-Content $AddinTemplate -Raw
        $addinContent = $addinContent -replace "<Assembly>MEPQCChecker.Revit.dll</Assembly>",
            "<Assembly>MEPQCChecker\MEPQCChecker.Revit.dll</Assembly>"
        Set-Content -Path $addinFile -Value $addinContent -Encoding UTF8
        Write-Host "  Created .addin manifest" -ForegroundColor DarkGray
    }

    Write-Host "  Done ($buildName build)" -ForegroundColor Green
    $installed++
}

Write-Host ""
if ($installed -gt 0) {
    Write-Host "Successfully installed for $installed Revit version(s)." -ForegroundColor Green
} else {
    Write-Host "No installations completed. Check build output paths." -ForegroundColor Yellow
}
Write-Host "Restart Revit to load the plugin.`n" -ForegroundColor Cyan
