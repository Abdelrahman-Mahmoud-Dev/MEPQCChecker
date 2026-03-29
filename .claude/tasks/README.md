# MEP QC Checker — Task Tracker

## Overall Progress

| Step | Task | Status | Files |
|------|------|--------|-------|
| 1 | Solution Setup | DONE | .sln, 4x .csproj |
| 2 | Data Models | DONE | 11 model classes |
| 3 | Check Classes + Tests | DONE | 5 checks, 32 tests passing |
| 4 | Ribbon + App Entry | DONE | App.cs, RibbonSetup.cs, RunQCCheckCommand.cs, ClearOverridesCommand.cs, MEPQCChecker.addin |
| 5 | RevitModelAdapter | DONE | RevitModelAdapter.cs (single Revit API bridge) |
| 6 | Color Overrides | DONE | ColorOverrideService.cs, HighlightPlan.cs |
| 7 | Dashboard UI | DONE | QCDashboardPanel.xaml/.cs, ZoomToElementHandler.cs, SeverityToColorConverter.cs |
| 8 | Config File Loading | DONE | config.json copied to output, 6 new ConfigService tests |
| 9 | Installer | DONE | Install.ps1, Uninstall.ps1 |
| 10 | CI Pipeline | DONE | .github/workflows/build.yml |

## Build Status

- **Full solution:** `dotnet build MEPQCChecker.sln -c Release` — 0 errors, 0 warnings (1 minor nullable warning in Core)
- **Unit tests:** 41/41 passing
- **Revit2024 (net48):** Builds clean
- **Revit2025 (net8.0-windows):** Builds clean

## Notes

- Steps 4-7 build-verified only (no Revit installed on this machine)
- Runtime testing requires Revit 2020-2026 with a live model
- Installer requires Revit installed to detect versions from registry
