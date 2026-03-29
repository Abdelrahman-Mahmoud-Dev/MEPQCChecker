# MEP QC Checker — Revit Plugin

A Revit add-in that gives MEP engineers a **single button** to scan their active model for quality issues, clashes, and missing data across **Mechanical**, **Plumbing**, and **Fire Protection** disciplines.

![Build & Test](https://github.com/Abdelrahman-Mahmoud-Dev/MEPQCChecker/actions/workflows/build.yml/badge.svg)

---

## Table of Contents

- [What It Does](#what-it-does)
- [Supported Revit Versions](#supported-revit-versions)
- [Quick Start](#quick-start)
- [Detailed Installation Guide](#detailed-installation-guide)
- [Usage Guide](#usage-guide)
- [Configuration](#configuration)
- [Project Structure](#project-structure)
- [Building from Source](#building-from-source)
- [Architecture](#architecture)
- [Troubleshooting](#troubleshooting)
- [Roadmap](#roadmap)

---

## What It Does

Click **Run QC Check** in the MEP Tools ribbon tab and within seconds see:

- A **dockable dashboard panel** inside Revit showing issue counts by severity
- Clashing elements **highlighted in red/amber** directly in the 3D view
- Every issue listed with an element ID so you can **click to zoom** to the problem

### The 5 QC Checks

| # | Check | What It Finds | Severity |
|---|-------|--------------|----------|
| 1 | **Clash Detection** | Intersecting bounding boxes between MEP elements using spatial grid partitioning (2m cells) | Critical |
| 2 | **Unconnected Elements** | Pipes/ducts with open connector ends (excludes terminals like fixtures & sprinklers) | Warning |
| 3 | **Missing Parameters** | Required parameters that are empty, null, or `<none>` — fully configurable per category via config.json | Configurable |
| 4 | **Pipe Slope** | Drainage pipes with incorrect slope — too flat, too steep, or wrong direction (gravity systems only) | Critical/Warning |
| 5 | **Sprinkler Coverage** | Room areas outside sprinkler head coverage radius using 0.5m grid sampling with point-in-polygon test | Critical/Warning |

### MVP Scope Boundaries

| Included | Not Included (Phase 2+) |
|----------|------------------------|
| Single-model QC scanning | Multi-model / linked-file clash detection |
| 3D view color highlights (red/amber) | Excel/PDF report export |
| Dockable dashboard with filtering | Automated fixing of issues |
| Click-to-zoom to any issue | Automation module (bulk parameter fill) |
| Runtime config.json customization | Standards/Family library browser |

---

## Supported Revit Versions

| Revit Version | .NET Framework | Build Target | Notes |
|---------------|---------------|-------------|-------|
| 2020 | .NET Framework 4.8 | `MEPQCChecker.Revit2024` | Same DLL for all 2020-2024 |
| 2021 | .NET Framework 4.8 | `MEPQCChecker.Revit2024` | |
| 2022 | .NET Framework 4.8 | `MEPQCChecker.Revit2024` | |
| 2023 | .NET Framework 4.8 | `MEPQCChecker.Revit2024` | |
| 2024 | .NET Framework 4.8 | `MEPQCChecker.Revit2024` | |
| 2025 | .NET 8 | `MEPQCChecker.Revit2025` | Same DLL for 2025-2026 |
| 2026 | .NET 8 | `MEPQCChecker.Revit2025` | |

---

## Quick Start

```bash
# 1. Clone
git clone https://github.com/Abdelrahman-Mahmoud-Dev/MEPQCChecker.git
cd MEPQCChecker

# 2. Build
dotnet build MEPQCChecker.sln -c Release

# 3. Install (run from repo root)
powershell -ExecutionPolicy Bypass -File installer\Install.ps1

# 4. Restart Revit → look for "MEP Tools" tab → click "Run QC Check"
```

---

## Detailed Installation Guide

### Prerequisites

Before building, install these on your machine:

| Tool | Required | Download Link | How to verify |
|------|----------|--------------|---------------|
| **Git** | Yes | https://git-scm.com/download/win | `git --version` |
| **.NET 8 SDK** | Yes | https://dotnet.microsoft.com/download/dotnet/8.0 | `dotnet --version` (should show 8.x) |
| **.NET Framework 4.8 Developer Pack** | Yes (for Revit 2020-2024) | https://dotnet.microsoft.com/download/dotnet-framework/net48 | Included in Visual Studio if installed |

> **Note:** You do NOT need Visual Studio. The `dotnet` CLI handles everything.

You can also install via command line:
```powershell
winget install Git.Git
winget install Microsoft.DotNet.SDK.8
winget install Microsoft.DotNet.Framework.DeveloperPack_4
```

### Step 1: Clone the Repository

```bash
git clone https://github.com/Abdelrahman-Mahmoud-Dev/MEPQCChecker.git
cd MEPQCChecker
```

### Step 2: Build in Release Mode

```bash
dotnet build MEPQCChecker.sln -c Release
```

You should see **Build succeeded** with all 4 projects:
```
MEPQCChecker.Core        -> ...\bin\Release\netstandard2.0\MEPQCChecker.Core.dll
MEPQCChecker.Core.Tests  -> ...\bin\Release\net8.0\MEPQCChecker.Core.Tests.dll
MEPQCChecker.Revit2024   -> ...\bin\Release\net48\MEPQCChecker.Revit.dll
MEPQCChecker.Revit2025   -> ...\bin\Release\net8.0-windows\MEPQCChecker.Revit.dll
```

If the build fails, check [Troubleshooting](#troubleshooting) below.

### Step 3: Install the Plugin

You have two options — automated or manual.

---

#### Option A: Automated Installer (Recommended)

> **Important:** Always run from the repository root folder, or use the `-BuildRoot` flag.

Open **PowerShell** and run:

```powershell
# Navigate to the repo root first
cd C:\path\to\MEPQCChecker

# Install for all detected Revit versions
powershell -ExecutionPolicy Bypass -File installer\Install.ps1
```

**Other install commands:**

```powershell
# Install for a specific Revit version only
powershell -ExecutionPolicy Bypass -File installer\Install.ps1 -RevitVersion 2024

# Run from any directory by passing -BuildRoot
powershell -ExecutionPolicy Bypass -File C:\MEPQCChecker\installer\Install.ps1 -BuildRoot C:\MEPQCChecker
```

**What the installer does:**

1. Finds `MEPQCChecker.sln` to verify the repo root
2. Detects installed Revit versions from Windows registry
3. Selects the correct build: `net48` for Revit 2020-2024, `net8` for Revit 2025-2026
4. Creates `%AppData%\Autodesk\Revit\Addins\{year}\MEPQCChecker\` folder
5. Copies plugin DLLs + `config.json` (skips Revit-owned DLLs like `RevitAPI.dll`)
6. Creates the `.addin` manifest in the Addins folder
7. Prints a summary of installed files per Revit version

**No admin rights required** — installs to your user profile only.

**Example output:**
```
============================================
  MEP QC Checker — Installer
============================================

Solution root: C:\MEPQCChecker

Detected Revit versions: 2023, 2024, 2025

── Revit 2023 ──
  + MEPQCChecker.Core.dll
  + MEPQCChecker.Revit.dll
  + Microsoft.Bcl.AsyncInterfaces.dll
  + System.Buffers.dll
  + System.Memory.dll
  + System.Numerics.Vectors.dll
  + System.Runtime.CompilerServices.Unsafe.dll
  + System.Text.Encodings.Web.dll
  + System.Text.Json.dll
  + System.Threading.Tasks.Extensions.dll
  + System.ValueTuple.dll
  + config.json
  + MEPQCChecker.addin (manifest)
  Installed 12 files (.NET Framework 4.8)
  Location: C:\Users\You\AppData\Roaming\Autodesk\Revit\Addins\2023\MEPQCChecker

── Revit 2025 ──
  + MEPQCChecker.Core.dll
  + MEPQCChecker.Revit.dll
  + config.json
  + MEPQCChecker.addin (manifest)
  Installed 3 files (.NET 8)
  Location: C:\Users\You\AppData\Roaming\Autodesk\Revit\Addins\2025\MEPQCChecker

============================================
  SUCCESS: Installed for 3 Revit version(s)

  Next steps:
  1. Restart Revit
  2. Look for 'MEP Tools' tab in the ribbon
  3. Click 'Run QC Check' to scan your model
============================================
```

---

#### Option B: Manual Installation

If the automated installer doesn't work or you prefer manual control, follow these steps:

**Step B1 — Find the correct build output folder:**

| Your Revit Version | Build output folder |
|--------------------|-------------------|
| 2020, 2021, 2022, 2023, 2024 | `src\MEPQCChecker.Revit2024\bin\Release\net48\` |
| 2025, 2026 | `src\MEPQCChecker.Revit2025\bin\Release\net8.0-windows\` |

**Step B2 — Create the plugin folder:**

Open File Explorer and navigate to:
```
C:\Users\<YourName>\AppData\Roaming\Autodesk\Revit\Addins\<year>\
```

> **Tip:** Type `%AppData%\Autodesk\Revit\Addins\` in the Explorer address bar.

Create a new folder called `MEPQCChecker` inside the year folder.

**Step B3 — Copy plugin files:**

Copy these files from the build output into the `MEPQCChecker` folder:

**Always required (all Revit versions):**

| File | Description |
|------|-------------|
| `MEPQCChecker.Revit.dll` | Main plugin assembly |
| `MEPQCChecker.Core.dll` | Business logic (check algorithms) |
| `config.json` | Runtime configuration |

**Also required for Revit 2020-2024 (net48 build only):**

| File | Description |
|------|-------------|
| `System.Text.Json.dll` | JSON parsing library |
| `System.Memory.dll` | Memory utilities |
| `System.Buffers.dll` | Buffer utilities |
| `System.Runtime.CompilerServices.Unsafe.dll` | Runtime helper |
| `System.Numerics.Vectors.dll` | Math vectors |
| `System.Text.Encodings.Web.dll` | Text encoding |
| `System.Threading.Tasks.Extensions.dll` | Async support |
| `System.ValueTuple.dll` | ValueTuple support |
| `Microsoft.Bcl.AsyncInterfaces.dll` | Async interfaces |

> **Warning — DO NOT copy these files** (they belong to Revit and will cause conflicts):
> - `RevitAPI.dll`
> - `RevitAPIUI.dll`
> - `AdWindows.dll`
> - `UIFramework.dll`

**Step B4 — Create the add-in manifest:**

Copy the file `installer\MEPQCChecker.addin` to:
```
C:\Users\<YourName>\AppData\Roaming\Autodesk\Revit\Addins\<year>\MEPQCChecker.addin
```

Open it in a text editor and change the `<Assembly>` line to:
```xml
<Assembly>MEPQCChecker\MEPQCChecker.Revit.dll</Assembly>
```

Full manifest content:
```xml
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
  <AddIn Type="Application">
    <Name>MEP QC Checker</Name>
    <Assembly>MEPQCChecker\MEPQCChecker.Revit.dll</Assembly>
    <FullClassName>MEPQCChecker.Revit.App</FullClassName>
    <ClientId>B7E3F1A2-4C8D-4F9E-A6B5-1D2E3F4A5B6C</ClientId>
    <VendorId>MEPTools</VendorId>
    <VendorDescription>MEP QC Checker Plugin</VendorDescription>
  </AddIn>
</RevitAddIns>
```

**Step B5 — Verify the final folder structure:**

```
C:\Users\YourName\AppData\Roaming\Autodesk\Revit\Addins\2024\
│
├── MEPQCChecker.addin                              ← manifest file
│
└── MEPQCChecker\                                   ← plugin folder
    ├── MEPQCChecker.Revit.dll                      ← main plugin
    ├── MEPQCChecker.Core.dll                       ← business logic
    ├── config.json                                 ← configuration
    ├── System.Text.Json.dll                        ← (net48 only)
    ├── System.Memory.dll                           ← (net48 only)
    ├── System.Buffers.dll                          ← (net48 only)
    ├── System.Runtime.CompilerServices.Unsafe.dll  ← (net48 only)
    ├── System.Numerics.Vectors.dll                 ← (net48 only)
    ├── System.Text.Encodings.Web.dll               ← (net48 only)
    ├── System.Threading.Tasks.Extensions.dll       ← (net48 only)
    ├── System.ValueTuple.dll                       ← (net48 only)
    └── Microsoft.Bcl.AsyncInterfaces.dll           ← (net48 only)
```

---

### Step 4: Restart Revit

1. Close Revit completely
2. Reopen Revit
3. Open any project
4. Look for the **"MEP Tools"** tab in the ribbon bar
5. You should see two buttons: **Run QC Check** and **Clear Highlights**

### Uninstalling

**Automated:**
```powershell
cd C:\path\to\MEPQCChecker
powershell -ExecutionPolicy Bypass -File installer\Uninstall.ps1
```

**Manual:** Delete these for each Revit version:
- Folder: `%AppData%\Autodesk\Revit\Addins\{year}\MEPQCChecker\`
- File: `%AppData%\Autodesk\Revit\Addins\{year}\MEPQCChecker.addin`

---

## Usage Guide

### Running a QC Check

1. Open a Revit model containing MEP elements (ducts, pipes, sprinklers, etc.)
2. Go to the **MEP Tools** tab in the ribbon
3. Click **Run QC Check**
4. Wait a few seconds while the model is scanned
5. A summary dialog appears:
   ```
   QC Check Complete

   Critical: 12
   Warning: 45
   Info: 8
   Total: 65 issues found
   ```

### Reviewing Results in the Dashboard

The **dashboard panel** docks to the right side of Revit and shows:

| Section | What It Shows |
|---------|--------------|
| **Summary Cards** | Three colored cards: Critical (red), Warning (amber), Info (gray) with counts |
| **Filter Bar** | Two dropdowns — filter by Discipline (All/Mechanical/Plumbing/FireProtection) or Severity (All/Critical/Warning/Info) |
| **Issue List** | Scrollable table with: severity dot, check type, description, element ID, level |
| **Action Bar** | Run Check, Clear Highlights, Export (disabled — Phase 2) |
| **Status Bar** | Last run time, total issue count, model name |

### Interacting with Issues

- **Click any issue** in the list to **zoom to and select** the element in the 3D view
- Elements with issues are **color-highlighted**:
  - **Red fill** = Critical severity (clashes, safety issues, wrong pipe direction)
  - **Amber fill** = Warning severity (missing parameters, excessive slope)
  - Info issues are shown in the panel only (no 3D highlight)

### Clearing Results

Click **Clear Highlights** in the ribbon to remove all color overrides from the active view.

---

## Configuration

Edit `config.json` in the plugin folder to customize check behavior **without recompiling**.

**Location:** `%AppData%\Autodesk\Revit\Addins\{year}\MEPQCChecker\config.json`

After editing, just re-run the QC check — **no Revit restart needed**.

### Required Parameters

Define which parameters must be filled for each Revit element category:

```json
{
  "RequiredParameters": {
    "OST_DuctCurves": [
      { "Name": "System Name",           "Severity": "Warning" },
      { "Name": "System Classification", "Severity": "Warning" },
      { "Name": "Insulation Type",       "Severity": "Info" },
      { "Name": "Flow",                  "Severity": "Warning" }
    ],
    "OST_Sprinklers": [
      { "Name": "System Name",     "Severity": "Critical" },
      { "Name": "Head Type",       "Severity": "Critical" },
      { "Name": "Coverage Radius", "Severity": "Warning" },
      { "Name": "Flow",            "Severity": "Warning" }
    ]
  }
}
```

**Severity values:** `"Critical"`, `"Warning"`, `"Info"`

**Available categories:** `OST_DuctCurves`, `OST_DuctFitting`, `OST_PipeCurves`, `OST_PipeFitting`, `OST_Sprinklers`, `OST_MechanicalEquipment`, `OST_PlumbingFixtures`

### Pipe Slope Thresholds

```json
{
  "PipeSlope": {
    "MinSlopePctLargePipe":  1.0,
    "MinSlopePctSmallPipe":  2.0,
    "MaxSlopePct":          15.0,
    "SmallPipeThresholdMM": 50
  }
}
```

| Setting | Default | Description |
|---------|---------|-------------|
| `MinSlopePctLargePipe` | 1.0% | Minimum slope for pipes >= 50mm diameter |
| `MinSlopePctSmallPipe` | 2.0% | Minimum slope for pipes < 50mm diameter |
| `MaxSlopePct` | 15.0% | Maximum slope before warning is raised |
| `SmallPipeThresholdMM` | 50 | Diameter threshold separating small/large pipes (mm) |

### Sprinkler Coverage

```json
{
  "SprinklerCoverage": {
    "DefaultCoverageRadiusM":  2.25,
    "CriticalUncoveredPct":    5.0,
    "WarningUncoveredPct":     1.0,
    "GridSamplingResolutionM": 0.5
  }
}
```

| Setting | Default | Description |
|---------|---------|-------------|
| `DefaultCoverageRadiusM` | 2.25m | Coverage radius per sprinkler head |
| `CriticalUncoveredPct` | 5% | Uncovered room area % that triggers Critical |
| `WarningUncoveredPct` | 1% | Uncovered room area % that triggers Warning |
| `GridSamplingResolutionM` | 0.5m | Sampling grid resolution (lower = more accurate, slower) |

### Gravity-Drained Systems

Only pipes with these system names are checked for slope violations:

```json
{
  "GravityDrainedSystemNames": [
    "Sanitary", "Vent", "Storm Drain", "Domestic Cold Water Return"
  ]
}
```

Add or remove system names to match your project's naming conventions.

---

## Project Structure

```
MEPQCChecker.sln
├── src/
│   ├── MEPQCChecker.Core/           # Business logic — zero Revit API dependency
│   │   ├── Checks/                  # 5 IQCCheck implementations
│   │   ├── Models/                  # QCIssue, QCReport, RevitModelSnapshot, etc.
│   │   └── Services/               # CheckRunner, ConfigService, HighlightPlan
│   ├── MEPQCChecker.Revit/          # Shared Revit source files (not a .csproj)
│   │   ├── Adapters/               # RevitModelAdapter — the only Revit API bridge
│   │   ├── Commands/               # RunQCCheckCommand, ClearOverridesCommand
│   │   ├── Ribbon/                 # RibbonSetup — tab and button creation
│   │   ├── Services/               # ColorOverrideService
│   │   └── UI/                     # WPF dashboard panel, zoom handler, converters
│   ├── MEPQCChecker.Revit2024/     # Build target: Revit 2020-2024 (net48)
│   └── MEPQCChecker.Revit2025/     # Build target: Revit 2025-2026 (net8)
├── tests/
│   └── MEPQCChecker.Core.Tests/    # 41 xUnit tests — runs without Revit
├── installer/
│   ├── Install.ps1                 # Automated installer
│   ├── Uninstall.ps1               # Automated uninstaller
│   └── MEPQCChecker.addin          # Add-in manifest template
└── .github/workflows/
    └── build.yml                   # CI pipeline (GitHub Actions)
```

---

## Building from Source

### Prerequisites

| Tool | Version | Install |
|------|---------|---------|
| Git | Any | `winget install Git.Git` |
| .NET 8 SDK | 8.0+ | `winget install Microsoft.DotNet.SDK.8` |
| .NET Fx 4.8 Dev Pack | 4.8+ | `winget install Microsoft.DotNet.Framework.DeveloperPack_4` |

### Clone & Build

```bash
git clone https://github.com/Abdelrahman-Mahmoud-Dev/MEPQCChecker.git
cd MEPQCChecker

# Build all projects
dotnet build MEPQCChecker.sln -c Release

# Or build specific targets
dotnet build src/MEPQCChecker.Revit2024 -c Release   # Revit 2020-2024 (net48)
dotnet build src/MEPQCChecker.Revit2025 -c Release   # Revit 2025-2026 (net8)
```

### Run Tests

```bash
dotnet test tests/MEPQCChecker.Core.Tests --verbosity normal
```

All 41 tests run **without Revit installed** — the Core project has zero Revit API dependency.

---

## Architecture

```
   Revit Application
        │
        ▼
┌──────────────────────────┐
│  RevitModelAdapter        │  ← ONLY class that touches Revit API
│  (FilteredElementCollector │    Converts feet → metres
│   → RevitModelSnapshot)   │    Extracts elements, rooms, levels
└──────────┬───────────────┘
           │  RevitModelSnapshot (plain C# object)
           ▼
┌──────────────────────────┐
│  CheckRunner              │  ← Pure C#, zero Revit dependency
│  ├── ClashDetector        │    Spatial grid partitioning (2m cells)
│  ├── UnconnectedChecker   │    Open connector detection
│  ├── MissingParamChecker  │    Config-driven parameter validation
│  ├── PipeSlopeChecker     │    Gravity drain slope analysis
│  └── SprinklerCovChecker  │    Room grid sampling + coverage calc
└──────────┬───────────────┘
           │  QCReport (issues list)
           ▼
┌──────────────────────────┐
│  ColorOverrideService     │  ← Red/amber fills in 3D view
│  QCDashboardPanel (WPF)   │  ← Dockable panel with results
│  ZoomToElementHandler     │  ← Thread-safe click-to-zoom
└──────────────────────────┘
```

**Key design decisions:**
- **Core separation** — all check logic in `MEPQCChecker.Core` (netstandard2.0), testable without Revit
- **Single adapter** — `RevitModelAdapter` is the only Revit API bridge
- **Thread safety** — WPF panel uses `IExternalEventHandler` + `ExternalEvent.Raise()` to call Revit API
- **Shared source** — `MEPQCChecker.Revit/` is a source folder (not a project); both build targets link to it
- **No Newtonsoft.Json** — uses System.Text.Json to avoid DLL conflicts with Revit

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| **"MEP Tools" tab doesn't appear in Revit** | Verify `.addin` file exists at `%AppData%\Autodesk\Revit\Addins\{year}\MEPQCChecker.addin` and the `<Assembly>` path points to `MEPQCChecker\MEPQCChecker.Revit.dll` |
| **Revit shows "assembly not found"** | All required DLLs must be in the `MEPQCChecker\` subfolder. For net48 builds, this includes `System.Text.Json.dll` and other `System.*.dll` files |
| **Plugin loads but crashes on Run QC Check** | Check the log file at `MEPQCChecker\MEPQCChecker.log` next to the plugin DLL |
| **"Could not load file or assembly 'System.Text.Json'"** | Copy ALL `System.*.dll` files from the net48 build output (Revit 2020-2024 only) |
| **Installer: "Cannot bind argument to parameter 'Path'"** | The script can't find the repo root. Run from the repo root folder: `cd C:\MEPQCChecker` then `.\installer\Install.ps1`, or pass `-BuildRoot C:\MEPQCChecker` |
| **Installer: "Cannot find MEPQCChecker.sln"** | You're running from the wrong folder. `cd` to the repo root, or use `-BuildRoot` parameter |
| **Installer: "No Revit installations found"** | Use `-RevitVersion 2024` to specify your Revit version manually |
| **Installer: "Build output not found"** | Run `dotnet build MEPQCChecker.sln -c Release` before running the installer |
| **"Execution of scripts is disabled"** | Run: `Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope CurrentUser` in PowerShell |
| **Build fails: "NETFramework,Version=v4.8 not found"** | Install the .NET Framework 4.8 Developer Pack: `winget install Microsoft.DotNet.Framework.DeveloperPack_4` |
| **Build fails: "dotnet not found"** | Install .NET 8 SDK: `winget install Microsoft.DotNet.SDK.8`, then restart your terminal |

---

## Roadmap

| Phase | Features | Status |
|-------|----------|--------|
| **Phase 1 (MVP)** | 5 QC checks, dashboard, 3D highlights, config.json, installer, CI | Done |
| **Phase 2** | Excel/PDF report export, linked-file clash detection, auto-fix proposals | In Progress |
| **Phase 3** | Automation module (bulk parameter fill, auto-tagging) | Planned |
| **Phase 4** | Standards/Family library browser | Planned |

---

## License

This project is proprietary. All rights reserved.
