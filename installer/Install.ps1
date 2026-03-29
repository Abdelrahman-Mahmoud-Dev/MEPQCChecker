#Requires -Version 5.1
<#
.SYNOPSIS
    Installs the MEP QC Checker Revit plugin for all detected Revit versions.

.DESCRIPTION
    Detects installed Revit versions via registry and copies the correct build
    (net48 for 2020-2024, net8 for 2025-2026) to each user's Revit Addins folder.
    No admin rights required.

.PARAMETER RevitVersion
    Optional. Install for a specific Revit version only (e.g., 2024).
    If not specified, installs for all detected versions.

.PARAMETER BuildRoot
    Optional. Path to the solution root folder. Defaults to parent of installer folder.

.EXAMPLE
    .\Install.ps1
    Installs for all detected Revit versions.

.EXAMPLE
    .\Install.ps1 -RevitVersion 2024
    Installs for Revit 2024 only.
#>

param(
    [int]$RevitVersion = 0,
    [string]$BuildRoot = ""
)

$ErrorActionPreference = "Stop"

# --- Resolve BuildRoot -----------------------------------------------
# $PSScriptRoot can be empty when invoked certain ways, so we try multiple fallbacks
if ([string]::IsNullOrEmpty($BuildRoot)) {
    # Try 1: $PSScriptRoot (works when dot-sourced or run directly)
    if (-not [string]::IsNullOrEmpty($PSScriptRoot)) {
        $BuildRoot = Split-Path -Parent $PSScriptRoot
    }
    # Try 2: $MyInvocation (works with powershell -File)
    elseif (-not [string]::IsNullOrEmpty($MyInvocation.MyCommand.Path)) {
        $BuildRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
    }
    # Try 3: Current directory
    else {
        $BuildRoot = (Get-Location).Path
        # Check if we're inside the installer folder
        if ((Split-Path -Leaf $BuildRoot) -eq "installer") {
            $BuildRoot = Split-Path -Parent $BuildRoot
        }
    }
}

# Verify BuildRoot looks correct
$slnPath = Join-Path $BuildRoot "MEPQCChecker.sln"
if (-not (Test-Path $slnPath)) {
    # Maybe user is running from the repo root
    $cwdSln = Join-Path (Get-Location).Path "MEPQCChecker.sln"
    if (Test-Path $cwdSln) {
        $BuildRoot = (Get-Location).Path
    }
    else {
        Write-Host "ERROR: Cannot find MEPQCChecker.sln" -ForegroundColor Red
        Write-Host "Run this script from the repository root, or pass -BuildRoot:" -ForegroundColor Yellow
        Write-Host "  .\installer\Install.ps1 -BuildRoot C:\path\to\MEPQCChecker" -ForegroundColor White
        Write-Host ""
        exit 1
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  MEP QC Checker - Installer" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Solution root: $BuildRoot" -ForegroundColor DarkGray
Write-Host ""

# --- Build output paths ----------------------------------------------
$Net48Output   = Join-Path $BuildRoot "src\MEPQCChecker.Revit2024\bin\Release\net48"
$Net8Output    = Join-Path $BuildRoot "src\MEPQCChecker.Revit2025\bin\Release\net8.0-windows"
$AddinTemplate = Join-Path $BuildRoot "installer\MEPQCChecker.addin"

# --- Verify build exists ---------------------------------------------
$hasNet48 = Test-Path $Net48Output
$hasNet8  = Test-Path $Net8Output

if (-not $hasNet48 -and -not $hasNet8) {
    Write-Host "ERROR: No build output found." -ForegroundColor Red
    Write-Host ""
    Write-Host "Please build the solution first:" -ForegroundColor Yellow
    Write-Host "  cd $BuildRoot" -ForegroundColor White
    Write-Host "  dotnet build MEPQCChecker.sln -c Release" -ForegroundColor White
    Write-Host ""
    exit 1
}

# --- DLLs that belong to Revit (must NOT be copied) ------------------
$revitOwnedDlls = @(
    "RevitAPI.dll",
    "RevitAPIUI.dll",
    "RevitAPIIFC.dll",
    "RevitAPIMacros.dll",
    "AdWindows.dll",
    "UIFramework.dll",
    "UIFrameworkServices.dll"
)

# --- Detect installed Revit versions ---------------------------------
$revitVersions = @()

if ($RevitVersion -gt 0) {
    # User specified a version
    $revitVersions = @($RevitVersion)
    Write-Host "Target: Revit $RevitVersion (user-specified)" -ForegroundColor White
} else {
    # Auto-detect from registry
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

    # Fallback: check existing Addins folders
    if ($revitVersions.Count -eq 0) {
        Write-Host "No Revit found in registry. Checking Addins folders..." -ForegroundColor Yellow
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
}

if ($revitVersions.Count -eq 0) {
    Write-Host "ERROR: No Revit installations found (2020-2026)." -ForegroundColor Red
    Write-Host ""
    Write-Host "Options:" -ForegroundColor Yellow
    Write-Host "  1. Install Revit first, then re-run this script" -ForegroundColor White
    Write-Host "  2. Specify a version manually:" -ForegroundColor White
    Write-Host "     .\Install.ps1 -RevitVersion 2024" -ForegroundColor White
    Write-Host ""
    exit 1
}

$revitVersions = $revitVersions | Sort-Object
Write-Host "Detected Revit versions: $($revitVersions -join ', ')" -ForegroundColor Green
Write-Host ""

# --- Install for each version ----------------------------------------
$installed = 0

foreach ($year in $revitVersions) {
    Write-Host "--- Revit $year ---" -ForegroundColor White

    # Determine which build to use
    if ($year -le 2024) {
        $sourceDir = $Net48Output
        $buildName = ".NET Framework 4.8"
        if (-not $hasNet48) {
            Write-Host "  SKIP: net48 build not found. Run:" -ForegroundColor Yellow
            Write-Host "    dotnet build src/MEPQCChecker.Revit2024 -c Release" -ForegroundColor White
            Write-Host ""
            continue
        }
    } else {
        $sourceDir = $Net8Output
        $buildName = ".NET 8"
        if (-not $hasNet8) {
            Write-Host "  SKIP: net8 build not found. Run:" -ForegroundColor Yellow
            Write-Host "    dotnet build src/MEPQCChecker.Revit2025 -c Release" -ForegroundColor White
            Write-Host ""
            continue
        }
    }

    # Target paths
    $addinsDir = Join-Path $env:APPDATA "Autodesk\Revit\Addins\$year"
    $pluginDir = Join-Path $addinsDir "MEPQCChecker"
    $addinFile = Join-Path $addinsDir "MEPQCChecker.addin"

    # Create directories
    if (-not (Test-Path $addinsDir)) {
        New-Item -ItemType Directory -Path $addinsDir -Force | Out-Null
    }
    if (-not (Test-Path $pluginDir)) {
        New-Item -ItemType Directory -Path $pluginDir -Force | Out-Null
    }

    # Copy plugin DLLs (exclude Revit-owned DLLs)
    $copiedCount = 0
    Get-ChildItem $sourceDir -Filter "*.dll" | Where-Object {
        $revitOwnedDlls -notcontains $_.Name
    } | ForEach-Object {
        Copy-Item $_.FullName -Destination $pluginDir -Force
        Write-Host "  + $($_.Name)" -ForegroundColor DarkGray
        $copiedCount++
    }

    # Copy config.json
    $configSrc = Join-Path $sourceDir "config.json"
    if (Test-Path $configSrc) {
        Copy-Item $configSrc -Destination $pluginDir -Force
        Write-Host "  + config.json" -ForegroundColor DarkGray
        $copiedCount++
    }

    # Create .addin manifest
    if (Test-Path $AddinTemplate) {
        $addinContent = Get-Content $AddinTemplate -Raw
        $addinContent = $addinContent -replace "<Assembly>MEPQCChecker.Revit.dll</Assembly>",
            "<Assembly>MEPQCChecker\MEPQCChecker.Revit.dll</Assembly>"
        Set-Content -Path $addinFile -Value $addinContent -Encoding UTF8
        Write-Host "  + MEPQCChecker.addin (manifest)" -ForegroundColor DarkGray
    }

    Write-Host "  Installed $copiedCount files ($buildName)" -ForegroundColor Green
    Write-Host "  Location: $pluginDir" -ForegroundColor DarkGray
    Write-Host ""
    $installed++
}

# --- Summary ---------------------------------------------------------
Write-Host "============================================" -ForegroundColor Cyan
if ($installed -gt 0) {
    Write-Host "  SUCCESS: Installed for $installed Revit version(s)" -ForegroundColor Green
    Write-Host ""
    Write-Host "  Next steps:" -ForegroundColor White
    Write-Host "  1. Restart Revit" -ForegroundColor White
    Write-Host "  2. Look for 'MEP Tools' tab in the ribbon" -ForegroundColor White
    Write-Host "  3. Click 'Run QC Check' to scan your model" -ForegroundColor White
} else {
    Write-Host "  No installations completed." -ForegroundColor Yellow
    Write-Host "  Check the warnings above." -ForegroundColor Yellow
}
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
