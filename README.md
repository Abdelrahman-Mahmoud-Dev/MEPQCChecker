# MEP QC Checker — Revit Plugin

A Revit add-in that gives MEP engineers a **single button** to scan their active model for quality issues, clashes, and missing data across **Mechanical**, **Plumbing**, and **Fire Protection** disciplines.

![Build & Test](https://github.com/Abdelrahman-Mahmoud-Dev/MEPQCChecker/actions/workflows/build.yml/badge.svg)

## What It Does

Click **Run QC Check** in the MEP Tools ribbon tab and within seconds see:

- A **dockable dashboard panel** inside Revit showing issue counts by severity
- Clashing elements **highlighted in red/amber** directly in the 3D view
- Every issue listed with an element ID so you can **click to zoom** to the problem

### The 5 QC Checks

| Check | What It Finds | Severity |
|-------|--------------|----------|
| **Clash Detection** | Intersecting bounding boxes between MEP elements using spatial grid partitioning | Critical |
| **Unconnected Elements** | Pipes/ducts with open connector ends (excludes terminals like fixtures & sprinklers) | Warning |
| **Missing Parameters** | Required parameters that are empty, null, or `<none>` (configurable per category) | Configurable |
| **Pipe Slope** | Drainage pipes with incorrect slope — too flat, too steep, or wrong direction | Critical/Warning |
| **Sprinkler Coverage** | Room areas outside sprinkler head coverage radius using 0.5m grid sampling | Critical/Warning |

### What It Does NOT Do (MVP Scope)

- No automated fixing or modification of elements
- No Excel/PDF report export (Phase 2)
- No multi-model / linked-file clash detection (Phase 2)
- No automation or standards library modules

## Supported Revit Versions

| Version | Framework | Build Target |
|---------|-----------|-------------|
| Revit 2020 - 2024 | .NET Framework 4.8 | `MEPQCChecker.Revit2024` |
| Revit 2025 - 2026 | .NET 8 | `MEPQCChecker.Revit2025` |

## Installation

### Option 1: PowerShell Installer (Recommended)

1. Build the solution in Release mode:
   ```
   dotnet build MEPQCChecker.sln -c Release
   ```

2. Run the installer:
   ```powershell
   powershell -File installer/Install.ps1
   ```

   The installer automatically:
   - Detects installed Revit versions from the registry
   - Copies the correct build (net48 or net8) for each version
   - Creates the `.addin` manifest in `%AppData%\Autodesk\Revit\Addins\{year}\`
   - **No admin rights required** — installs to per-user AppData

3. Restart Revit.

### Option 2: Manual Installation

1. Build the solution in Release mode.

2. Copy the appropriate build output to a folder:
   - For Revit 2020-2024: `src/MEPQCChecker.Revit2024/bin/Release/net48/`
   - For Revit 2025-2026: `src/MEPQCChecker.Revit2025/bin/Release/net8.0-windows/`

3. Create a folder at:
   ```
   %AppData%\Autodesk\Revit\Addins\{year}\MEPQCChecker\
   ```

4. Copy these files into it:
   - `MEPQCChecker.Revit.dll`
   - `MEPQCChecker.Core.dll`
   - `config.json`
   - All `System.*.dll` files (for Revit 2020-2024 only)

5. Copy `installer/MEPQCChecker.addin` to:
   ```
   %AppData%\Autodesk\Revit\Addins\{year}\MEPQCChecker.addin
   ```
   Edit the `<Assembly>` path to point to `MEPQCChecker\MEPQCChecker.Revit.dll`.

6. Restart Revit.

### Uninstall

```powershell
powershell -File installer/Uninstall.ps1
```

## Usage

1. Open a Revit model containing MEP elements.
2. Go to the **MEP Tools** tab in the ribbon.
3. Click **Run QC Check**.
4. Review results in the docked dashboard panel:
   - **Summary cards** show Critical / Warning / Info counts
   - **Filter** by discipline (Mechanical, Plumbing, Fire Protection) or severity
   - **Click any issue** to zoom to and select the element in the model
5. Elements with issues are highlighted in the 3D view:
   - **Red** = Critical (clashes, safety issues)
   - **Amber** = Warning (missing data, slope violations)
6. Click **Clear Highlights** to remove all color overrides.

## Configuration

Edit `config.json` (located next to the DLL) to customize check behavior **without recompiling**.

### Required Parameters

Define which parameters must be filled for each element category:

```json
{
  "RequiredParameters": {
    "OST_DuctCurves": [
      { "Name": "System Name", "Severity": "Warning" },
      { "Name": "Flow", "Severity": "Warning" }
    ],
    "OST_Sprinklers": [
      { "Name": "System Name", "Severity": "Critical" },
      { "Name": "Head Type", "Severity": "Critical" }
    ]
  }
}
```

### Pipe Slope Thresholds

```json
{
  "PipeSlope": {
    "MinSlopePctLargePipe": 1.0,
    "MinSlopePctSmallPipe": 2.0,
    "MaxSlopePct": 15.0,
    "SmallPipeThresholdMM": 50
  }
}
```

### Sprinkler Coverage

```json
{
  "SprinklerCoverage": {
    "DefaultCoverageRadiusM": 2.25,
    "CriticalUncoveredPct": 5.0,
    "WarningUncoveredPct": 1.0,
    "GridSamplingResolutionM": 0.5
  }
}
```

### Gravity-Drained Systems

Only these system names are checked for pipe slope:

```json
{
  "GravityDrainedSystemNames": [
    "Sanitary", "Vent", "Storm Drain", "Domestic Cold Water Return"
  ]
}
```

## Project Structure

```
MEPQCChecker.sln
├── src/
│   ├── MEPQCChecker.Core/           # Business logic — zero Revit API dependency
│   │   ├── Checks/                  # 5 IQCCheck implementations
│   │   ├── Models/                  # QCIssue, QCReport, RevitModelSnapshot
│   │   └── Services/               # CheckRunner, ConfigService, HighlightPlan
│   ├── MEPQCChecker.Revit/          # Shared Revit source files (not a project)
│   │   ├── Adapters/               # RevitModelAdapter — the only Revit API bridge
│   │   ├── Commands/               # RunQCCheck, ClearOverrides
│   │   ├── Ribbon/                 # Tab and button setup
│   │   ├── Services/               # ColorOverrideService
│   │   └── UI/                     # WPF dashboard panel, zoom handler
│   ├── MEPQCChecker.Revit2024/     # Build target: Revit 2020-2024 (net48)
│   └── MEPQCChecker.Revit2025/     # Build target: Revit 2025-2026 (net8)
├── tests/
│   └── MEPQCChecker.Core.Tests/    # 41 xUnit tests — runs without Revit
├── installer/
│   ├── Install.ps1                 # Automated installer
│   ├── Uninstall.ps1               # Automated uninstaller
│   └── MEPQCChecker.addin          # Add-in manifest template
└── .github/workflows/
    └── build.yml                   # CI pipeline
```

## Building from Source

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [.NET Framework 4.8 Developer Pack](https://dotnet.microsoft.com/download/dotnet-framework/net48) (for Revit 2020-2024 build)

### Build

```bash
# Build everything
dotnet build MEPQCChecker.sln -c Release

# Build specific target
dotnet build src/MEPQCChecker.Revit2024 -c Release   # net48
dotnet build src/MEPQCChecker.Revit2025 -c Release   # net8
```

### Test

```bash
dotnet test tests/MEPQCChecker.Core.Tests --verbosity normal
```

All 41 tests run without Revit installed — the Core project has zero Revit API dependency.

## Architecture

The key design principle is **Core separation**:

- **`MEPQCChecker.Core`** contains all check logic and operates on a `RevitModelSnapshot` — a plain C# object with no Revit types. This makes all business logic unit-testable without Revit.

- **`RevitModelAdapter`** is the **only class** that touches the Revit API. It reads the live model and builds the snapshot, converting all units from Revit internal (feet) to metric (metres).

- **`MEPQCChecker.Revit/`** is a shared source folder. Both `Revit2024` (net48) and `Revit2025` (net8) link to these files, producing `MEPQCChecker.Revit.dll` for their respective frameworks.

- The **WPF dashboard** never calls Revit API directly. Click-to-zoom uses `IExternalEventHandler` + `ExternalEvent.Raise()` for thread safety.

## Roadmap

- **Phase 2:** Excel/PDF report export, linked-file clash detection
- **Phase 3:** Automation module (bulk parameter fill, tagging)
- **Phase 4:** Standards/Family library browser

## License

This project is proprietary. All rights reserved.
