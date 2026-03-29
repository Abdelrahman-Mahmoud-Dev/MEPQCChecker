#Requires -Version 5.1
<#
.SYNOPSIS
    Uninstalls the MEP QC Checker Revit plugin from all Revit versions.
#>

$ErrorActionPreference = "Stop"

Write-Host "`n=== MEP QC Checker Uninstaller ===" -ForegroundColor Cyan
Write-Host ""

$addinsBase = Join-Path $env:APPDATA "Autodesk\Revit\Addins"
$removed = 0

if (-not (Test-Path $addinsBase)) {
    Write-Host "No Revit Addins folder found." -ForegroundColor Yellow
    exit 0
}

Get-ChildItem $addinsBase -Directory | ForEach-Object {
    $year = $_.Name
    $pluginDir = Join-Path $_.FullName "MEPQCChecker"
    $addinFile = Join-Path $_.FullName "MEPQCChecker.addin"

    $found = $false

    if (Test-Path $pluginDir) {
        Remove-Item $pluginDir -Recurse -Force
        Write-Host "Removed plugin folder: Revit $year" -ForegroundColor Green
        $found = $true
    }

    if (Test-Path $addinFile) {
        Remove-Item $addinFile -Force
        Write-Host "Removed .addin manifest: Revit $year" -ForegroundColor Green
        $found = $true
    }

    if ($found) { $removed++ }
}

Write-Host ""
if ($removed -gt 0) {
    Write-Host "Uninstalled from $removed Revit version(s).`n" -ForegroundColor Green
} else {
    Write-Host "MEP QC Checker was not found in any Revit version.`n" -ForegroundColor Yellow
}
