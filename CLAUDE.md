# MEP QC Checker — Revit Plugin

## Overview

Revit add-in for MEP quality checking. Scans active model for clashes, unconnected elements,
missing parameters, pipe slope violations, and sprinkler coverage gaps. Displays results in a
dockable WPF dashboard with color-coded 3D view highlights.

## Tech Stack

| Layer           | Tech                                              |
|-----------------|---------------------------------------------------|
| Core Logic      | C# / .NET Standard 2.0 (no Revit dependency)      |
| Revit API       | Revit_All_Main_Versions_API_x64 NuGet package     |
| Revit 2020-2024 | .NET Framework 4.8 (net48)                        |
| Revit 2025-2026 | .NET 8 (net8.0-windows)                           |
| UI              | WPF (dockable pane)                               |
| Config          | System.Text.Json (config.json)                    |
| Tests           | xUnit                                             |
| CI              | GitHub Actions (windows-latest)                   |

## Project Structure

```
MEPQCChecker.sln
├── src/
│   ├── MEPQCChecker.Core/           ← Business logic, NO Revit API
│   │   ├── Checks/                  ← 5 IQCCheck implementations
│   │   ├── Models/                  ← QCIssue, QCReport, RevitModelSnapshot
│   │   └── Services/               ← CheckRunner, ConfigService
│   ├── MEPQCChecker.Revit/          ← Shared source folder (NOT a .csproj)
│   │   ├── Commands/               ← IExternalCommand implementations
│   │   ├── UI/                     ← WPF XAML views
│   │   ├── Ribbon/                 ← RibbonSetup.cs
│   │   ├── Adapters/               ← RevitModelAdapter.cs
│   │   └── App.cs                  ← IExternalApplication entry point
│   ├── MEPQCChecker.Revit2024/     ← net48 build target, links to Revit/
│   └── MEPQCChecker.Revit2025/     ← net8.0-windows build target, links to Revit/
├── tests/
│   └── MEPQCChecker.Core.Tests/    ← xUnit, runs without Revit
├── installer/
│   └── Install.ps1
└── .github/workflows/
    └── build.yml
```

## Key Commands

```bash
dotnet build MEPQCChecker.sln -c Release                    # Build all
dotnet build src/MEPQCChecker.Revit2024 -c Release           # Build net48 only
dotnet build src/MEPQCChecker.Revit2025 -c Release           # Build net8 only
dotnet test tests/MEPQCChecker.Core.Tests --verbosity normal # Run unit tests
```

## Architecture Rules

1. **Core project has ZERO Revit API references** — all check logic operates on `RevitModelSnapshot`
2. **RevitModelAdapter is the ONLY class that touches Revit API** — it builds the snapshot
3. **WPF panel must NEVER call Revit API directly** — use `IExternalEventHandler` + `ExternalEvent.Raise()`
4. **Every IExternalCommand.Execute must wrap in try/catch** — log to file, show friendly dialog
5. **All units in Core are metric (metres)** — adapter converts from Revit internal (feet × 0.3048)
6. **config.json is loaded at runtime** — never hardcode thresholds or parameter lists
7. **Clash detection uses spatial grid partitioning** — 2m × 2m cells, not naive O(n²)
8. **No Newtonsoft.Json** — use System.Text.Json only to avoid DLL conflicts with Revit

## Multi-Target Build

- `MEPQCChecker.Revit/` is a shared source folder, NOT a separate project
- Revit2024 and Revit2025 projects link to those source files via `<Compile Include="..">`
- Both produce `MEPQCChecker.Revit.dll` as the output assembly name
- Core targets `netstandard2.0` so both net48 and net8 can reference it
- Use `#if REVIT2024` / `#if REVIT2025` for version-specific API differences (unlikely in MVP)

## Feature Specs

| Feature | Spec File | Status |
|---------|-----------|--------|
| Clash Detection | `.claude/docs/clash-detection.md` | Implemented |
| Unconnected Elements | `.claude/docs/unconnected-elements.md` | Implemented |
| Missing Parameters | `.claude/docs/missing-parameters.md` | Implemented |
| Pipe Slope Check | `.claude/docs/pipe-slope.md` | Implemented |
| Sprinkler Coverage | `.claude/docs/sprinkler-coverage.md` | Implemented |
| Dashboard UI | `.claude/docs/dashboard-ui.md` | Implemented |
| Ribbon Integration | `.claude/docs/ribbon-integration.md` | Implemented |
| Config System | `.claude/docs/config-system.md` | Implemented |

## Full Specification

See `plam.md` for the complete, detailed MVP specification including data models, algorithms, UI layout, installer logic, and implementation order.
