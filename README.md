# MEP QC Checker — Revit Plugin

A Revit add-in that gives MEP engineers a **single button** to scan their active model for quality issues, clashes, and missing data across **Mechanical**, **Plumbing**, and **Fire Protection** disciplines.

![Build & Test](https://github.com/Abdelrahman-Mahmoud-Dev/MEPQCChecker/actions/workflows/build.yml/badge.svg)

---

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

---

## Supported Revit Versions

| Revit Version | Framework | Build Target |
|---------------|-----------|-------------|
| 2020, 2021, 2022, 2023, 2024 | .NET Framework 4.8 | `MEPQCChecker.Revit2024` |
| 2025, 2026 | .NET 8 | `MEPQCChecker.Revit2025` |

---

## Installation

### Prerequisites

You need **one** of the following on the machine where you will build the plugin:

| Tool | Required For | Download |
|------|-------------|----------|
| .NET 8 SDK | Building the solution | https://dotnet.microsoft.com/download/dotnet/8.0 |
| .NET Framework 4.8 Developer Pack | Building the Revit 2020-2024 target | https://dotnet.microsoft.com/download/dotnet-framework/net48 |

You do **NOT** need Visual Studio — the `dotnet` CLI is sufficient.

### Step 1: Clone the Repository

```bash
git clone https://github.com/Abdelrahman-Mahmoud-Dev/MEPQCChecker.git
cd MEPQCChecker
```

### Step 2: Build in Release Mode

```bash
dotnet build MEPQCChecker.sln -c Release
```

Expected output — all 4 projects should build successfully:
```
MEPQCChecker.Core        -> ...\bin\Release\netstandard2.0\MEPQCChecker.Core.dll
MEPQCChecker.Core.Tests  -> ...\bin\Release\net8.0\MEPQCChecker.Core.Tests.dll
MEPQCChecker.Revit2024   -> ...\bin\Release\net48\MEPQCChecker.Revit.dll
MEPQCChecker.Revit2025   -> ...\bin\Release\net8.0-windows\MEPQCChecker.Revit.dll
```

### Step 3: Install the Plugin

#### Option A: Automated Installer (Recommended)

Open **PowerShell** and run:

```powershell
# Install for all detected Revit versions
powershell -ExecutionPolicy Bypass -File installer\Install.ps1

# Or install for a specific version only
powershell -ExecutionPolicy Bypass -File installer\Install.ps1 -RevitVersion 2024
```

The installer will:
1. Detect your installed Revit versions from the Windows registry
2. Select the correct build (`net48` for 2020-2024, `net8` for 2025-2026)
3. Copy plugin files to `%AppData%\Autodesk\Revit\Addins\{year}\MEPQCChecker\`
4. Create the `.addin` manifest file
5. Print a summary of what was installed

**No admin rights required** — everything goes to your user profile.

Example output:
```
============================================
  MEP QC Checker — Installer
============================================

Detected Revit versions: 2023, 2024, 2025

── Revit 2023 ──
  + MEPQCChecker.Core.dll
  + MEPQCChecker.Revit.dll
  + System.Text.Json.dll
  + config.json
  + MEPQCChecker.addin (manifest)
  Installed 8 files (.NET Framework 4.8)
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
============================================
```

#### Option B: Manual Installation

If the automated installer doesn't work or you prefer manual control:

**1. Locate the build output:**

| Your Revit Version | Copy files from |
|--------------------|----------------|
| 2020, 2021, 2022, 2023, 2024 | `src\MEPQCChecker.Revit2024\bin\Release\net48\` |
| 2025, 2026 | `src\MEPQCChecker.Revit2025\bin\Release\net8.0-windows\` |

**2. Create the plugin folder:**

```
%AppData%\Autodesk\Revit\Addins\{year}\MEPQCChecker\
```

For example, for Revit 2024:
```
C:\Users\YourName\AppData\Roaming\Autodesk\Revit\Addins\2024\MEPQCChecker\
```

**3. Copy these files** from the build output into the plugin folder:

| File | Required | Notes |
|------|----------|-------|
| `MEPQCChecker.Revit.dll` | Yes | Main plugin DLL |
| `MEPQCChecker.Core.dll` | Yes | Business logic |
| `config.json` | Yes | Check configuration |
| `System.Text.Json.dll` | net48 only | JSON parsing (Revit 2020-2024) |
| `System.Memory.dll` | net48 only | Dependency |
| `System.Buffers.dll` | net48 only | Dependency |
| `System.Runtime.CompilerServices.Unsafe.dll` | net48 only | Dependency |
| `System.Numerics.Vectors.dll` | net48 only | Dependency |
| `System.Text.Encodings.Web.dll` | net48 only | Dependency |
| `System.Threading.Tasks.Extensions.dll` | net48 only | Dependency |
| `System.ValueTuple.dll` | net48 only | Dependency |
| `Microsoft.Bcl.AsyncInterfaces.dll` | net48 only | Dependency |

**DO NOT copy** these (they belong to Revit):
- `RevitAPI.dll`
- `RevitAPIUI.dll`
- `AdWindows.dll`
- `UIFramework.dll`

**4. Create the `.addin` manifest file:**

Copy `installer\MEPQCChecker.addin` to:
```
%AppData%\Autodesk\Revit\Addins\{year}\MEPQCChecker.addin
```

Then edit the `<Assembly>` line to:
```xml
<Assembly>MEPQCChecker\MEPQCChecker.Revit.dll</Assembly>
```

The final manifest should look like:
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

**5. Verify your folder structure:**

```
%AppData%\Autodesk\Revit\Addins\2024\
├── MEPQCChecker.addin                    <-- manifest (in Addins root)
└── MEPQCChecker\                         <-- plugin folder
    ├── MEPQCChecker.Revit.dll
    ├── MEPQCChecker.Core.dll
    ├── config.json
    ├── System.Text.Json.dll              (net48 only)
    ├── System.Memory.dll                 (net48 only)
    └── ... other System.*.dll            (net48 only)
```

### Step 4: Restart Revit

Close and reopen Revit. You should see the **"MEP Tools"** tab in the ribbon.

### Troubleshooting Installation

| Problem | Solution |
|---------|----------|
| "MEP Tools" tab doesn't appear | Check that the `.addin` file is in the correct Addins folder and the `<Assembly>` path is correct |
| Revit shows "assembly not found" error | Make sure ALL required DLLs are in the `MEPQCChecker\` subfolder |
| Plugin loads but crashes | Check the log file at `MEPQCChecker\MEPQCChecker.log` next to the DLL |
| "Could not load file or assembly 'System.Text.Json'" | You're missing dependency DLLs — copy ALL `System.*.dll` files from the build output (net48 only) |
| Installer says "No Revit installations found" | Use `-RevitVersion 2024` parameter to specify your version manually |
| "Execution of scripts is disabled" error | Run: `Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope CurrentUser` |

### Uninstall

```powershell
powershell -ExecutionPolicy Bypass -File installer\Uninstall.ps1
```

Or manually delete:
- `%AppData%\Autodesk\Revit\Addins\{year}\MEPQCChecker\` folder
- `%AppData%\Autodesk\Revit\Addins\{year}\MEPQCChecker.addin` file

---

## Usage

### Running a QC Check

1. Open a Revit model containing MEP elements (ducts, pipes, sprinklers, etc.)
2. Go to the **MEP Tools** tab in the ribbon
3. Click **Run QC Check**
4. Wait a few seconds while the model is scanned
5. A summary dialog shows the results:
   ```
   QC Check Complete

   Critical: 12
   Warning: 45
   Info: 8
   Total: 65 issues found
   ```

### Reviewing Results

- The **dashboard panel** (docked right) shows:
  - **Summary cards** — Critical (red), Warning (amber), Info (gray) counts
  - **Filter dropdowns** — filter by Discipline or Severity
  - **Issue list** — every issue with check type, description, element ID, and level
- **Click any issue** in the list to zoom to and select the element in the model
- Elements are **color-coded in the 3D view**:
  - **Red** = Critical (clashes, wrong pipe direction, missing sprinkler data)
  - **Amber** = Warning (missing parameters, excessive slope)

### Clearing Results

Click **Clear Highlights** in the ribbon to remove all color overrides from the view.

---

## Configuration

Edit `config.json` (located in the `MEPQCChecker\` folder next to the DLL) to customize check behavior **without recompiling**.

After editing, just re-run the QC check — no Revit restart needed.

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

Severity values: `"Critical"`, `"Warning"`, `"Info"`

Available categories: `OST_DuctCurves`, `OST_DuctFitting`, `OST_PipeCurves`, `OST_PipeFitting`, `OST_Sprinklers`, `OST_MechanicalEquipment`, `OST_PlumbingFixtures`

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
| `MaxSlopePct` | 15.0% | Maximum slope before warning |
| `SmallPipeThresholdMM` | 50 | Diameter threshold in mm |

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
| `CriticalUncoveredPct` | 5% | Room uncovered area % that triggers Critical |
| `WarningUncoveredPct` | 1% | Room uncovered area % that triggers Warning |
| `GridSamplingResolutionM` | 0.5m | Grid sampling resolution (lower = more accurate but slower) |

### Gravity-Drained Systems

Only pipes with these system names are checked for slope:

```json
{
  "GravityDrainedSystemNames": [
    "Sanitary", "Vent", "Storm Drain", "Domestic Cold Water Return"
  ]
}
```

Add or remove system names to match your project naming conventions.

---

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

---

## Building from Source

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (required)
- [.NET Framework 4.8 Developer Pack](https://dotnet.microsoft.com/download/dotnet-framework/net48) (required for Revit 2020-2024 build)
- Git

### Build

```bash
git clone https://github.com/Abdelrahman-Mahmoud-Dev/MEPQCChecker.git
cd MEPQCChecker

# Build everything
dotnet build MEPQCChecker.sln -c Release

# Or build a specific target
dotnet build src/MEPQCChecker.Revit2024 -c Release   # Revit 2020-2024
dotnet build src/MEPQCChecker.Revit2025 -c Release   # Revit 2025-2026
```

### Test

```bash
dotnet test tests/MEPQCChecker.Core.Tests --verbosity normal
```

All 41 tests run **without Revit installed** — the Core project has zero Revit API dependency.

---

## Architecture

The key design principle is **Core separation**:

```
   Revit Application
        │
        ▼
┌─────────────────────┐
│  RevitModelAdapter   │  ← Only class that touches Revit API
│  (reads model →      │    Converts feet → metres
│   builds snapshot)   │
└────────┬────────────┘
         │  RevitModelSnapshot (plain C# object)
         ▼
┌─────────────────────┐
│  CheckRunner         │  ← Pure C#, no Revit dependency
│  ├─ ClashDetector    │    Runs all 5 checks
│  ├─ UnconnectedChk   │    Returns QCReport
│  ├─ MissingParamChk  │
│  ├─ PipeSlopeChk     │
│  └─ SprinklerCovChk  │
└────────┬────────────┘
         │  QCReport
         ▼
┌─────────────────────┐
│  ColorOverrideService│  ← Applies red/amber highlights
│  QCDashboardPanel    │  ← WPF panel with results
└─────────────────────┘
```

- **`MEPQCChecker.Core`** contains all check logic and operates on `RevitModelSnapshot` — making everything unit-testable without Revit.
- **`RevitModelAdapter`** is the **only class** that touches the Revit API.
- The **WPF dashboard** never calls Revit API directly — uses `IExternalEventHandler` for thread safety.
- **`MEPQCChecker.Revit/`** is a shared source folder. Both build targets link to these files.

---

## Roadmap

- **Phase 2:** Excel/PDF report export, linked-file clash detection
- **Phase 3:** Automation module (bulk parameter fill, tagging)
- **Phase 4:** Standards/Family library browser

## License

This project is proprietary. All rights reserved.
