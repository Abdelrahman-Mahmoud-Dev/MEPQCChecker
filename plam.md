# Overview

This is the complete, implementation-ready MVP plan for the **MEP QC Checker** Revit plugin — specifically the **Model Quality & Clash Detection** module.

> **Scope:** QC Checker module only. Reports, Automation, and Standards modules are deferred to later phases.
> 

| Field | Value |
| --- | --- |
| Disciplines | Mechanical · Plumbing · Fire Protection |
| Revit versions | 2020 · 2021 · 2022 · 2023 · 2024 · 2025 · 2026 |
| Language | C# / .NET Framework 4.8 + .NET 8 |
| Target audience | Mixed — technical & non-technical engineers |
| Estimated build time | 3–4 weeks (solo developer) |

---

# 1. What we are building

The MEP QC Checker is a Revit add-in that gives engineers a **single button** to scan their active model for quality issues, clashes, and missing data across Mechanical (HVAC/ducts), Plumbing (pipes/drainage/fixtures), and Fire Protection (sprinkler) disciplines.

The engineer opens a Revit model, clicks **Run QC Check** in the MEP Tools ribbon tab, and within seconds sees:

- A live dashboard panel docked inside Revit showing issue counts by type and severity
- Clashing elements highlighted in red directly in the 3D view
- Every issue listed with an element ID so engineers can click directly to the problem

## 1.1 What the plugin does NOT do in MVP

- No automated fixing or modification of elements
- No Reports module (Excel/PDF) — deferred to Phase 2
- No Automation module (bulk parameter fill, tagging)
- No Standards/Family library browser
- No multi-model or linked-file clash detection (Phase 2)

> ⚠️ **Note:** Linked file clash detection is the #1 feature request after MVP — deliberately excluded to keep the build focused and shippable.
> 

---

# 2. Project structure & solution layout

The solution uses **4 projects** supporting multiple Revit versions via shared code and version-specific build targets.

## 2.1 Solution file layout

```
MEPQCChecker.sln
│
├── src/
│   ├── MEPQCChecker.Core/           ← All business logic (NO Revit API)
│   │   ├── Checks/
│   │   │   ├── ClashDetector.cs
│   │   │   ├── UnconnectedElementChecker.cs
│   │   │   ├── MissingParameterChecker.cs
│   │   │   ├── PipeSlopeChecker.cs
│   │   │   └── SprinklerCoverageChecker.cs
│   │   ├── Models/
│   │   │   ├── QCIssue.cs
│   │   │   ├── QCSeverity.cs
│   │   │   └── QCReport.cs
│   │   └── Services/
│   │       ├── CheckRunner.cs
│   │       └── ColorOverrideService.cs
│   ├── MEPQCChecker.Revit/          ← Revit API layer
│   │   ├── Commands/
│   │   │   ├── RunQCCheckCommand.cs
│   │   │   └── ClearOverridesCommand.cs
│   │   ├── UI/
│   │   │   ├── QCDashboardPanel.xaml
│   │   │   └── IssueListView.xaml
│   │   ├── Ribbon/
│   │   │   └── RibbonSetup.cs
│   │   ├── Adapters/
│   │   │   └── RevitModelAdapter.cs
│   │   └── App.cs
│   ├── MEPQCChecker.Revit2024/      ← Build target Revit 2020-2024 (.NET 4.8)
│   └── MEPQCChecker.Revit2025/      ← Build target Revit 2025-2026 (.NET 8)
├── tests/
│   └── MEPQCChecker.Core.Tests/
├── installer/
│   └── Install.ps1
└── .github/workflows/
    └── build.yml
```

## 2.2 Key architectural decision: Core separation

All check logic lives in `MEPQCChecker.Core` which has **zero dependency on the Revit API**. This means:

- Core logic can be **unit-tested without Revit installed**
- New Revit versions only require a new build target, not code rewrites
- `RevitModelAdapter.cs` is the **only file** that touches the Revit API directly

---

# 3. Data models

Live in `MEPQCChecker.Core/Models/`.

## 3.1 QCSeverity.cs

```csharp
namespace MEPQCChecker.Core.Models
{
    public enum QCSeverity
    {
        Critical = 0,   // Clash, safety issue
        Warning  = 1,   // Missing data, slope violation
        Info     = 2    // Best practice suggestion
    }
}
```

## 3.2 QCIssue.cs

```csharp
public class QCIssue
{
    public string      IssueId         { get; set; }  // Unique GUID
    public QCSeverity  Severity         { get; set; }
    public string      CheckType        { get; set; }  // e.g. 'ClashDetection'
    public string      Discipline       { get; set; }  // 'Mechanical' | 'Plumbing' | 'FireProtection'
    public string      Description      { get; set; }
    public long        ElementId        { get; set; }  // Primary element
    public long?       ElementId2       { get; set; }  // Second element (clashes)
    public string      ElementCategory  { get; set; }
    public string      Level            { get; set; }
    public double?     MeasuredValue    { get; set; }
    public double?     RequiredValue    { get; set; }
    public string      ParameterName    { get; set; }
}
```

## 3.3 QCReport.cs

```csharp
public class QCReport
{
    public string          ProjectName  { get; set; }
    public string          ModelPath    { get; set; }
    public DateTime        RunAt        { get; set; } = DateTime.Now;
    public string          RevitVersion { get; set; }
    public List<QCIssue>   Issues       { get; set; } = new();

    public int CriticalCount => Issues.Count(i => i.Severity == QCSeverity.Critical);
    public int WarningCount  => Issues.Count(i => i.Severity == QCSeverity.Warning);
    public int InfoCount     => Issues.Count(i => i.Severity == QCSeverity.Info);
    public int TotalCount    => Issues.Count;
}
```

---

# 4. The five check classes

Each implements `IQCCheck`. All five are required for MVP.

## 4.0 IQCCheck interface

```csharp
public interface IQCCheck
{
    string CheckName  { get; }
    string Discipline { get; }  // 'All' | 'Mechanical' | 'Plumbing' | 'FireProtection'
    IEnumerable<QCIssue> Run(RevitModelSnapshot snapshot);
}
```

## 4.1 ClashDetector.cs

**Purpose:** Finds intersecting bounding boxes between MEP elements.

**Algorithm:**

- Collect all elements with a 3D bounding box from: Ducts, Pipes, CableTray, Conduit, PipeInsulation, DuctInsulation, sprinkler pipe categories
- For each element pair, check bounding box overlap in all three axes (X, Y, Z)
- Use **spatial grid partitioning**: group by level then by 2m × 2m XY grid cell. Only compare elements in same or adjacent cells. Reduces O(n²) to manageable even on large models
- Skip self-comparison and already-reported pairs
- One `QCIssue` per clash pair, `Severity = Critical`, both ElementIds populated

**Output:** `Clash between [CategoryA] (ID: {id1}) and [CategoryB] (ID: {id2}) on Level {level}`

> ⚠️ Do NOT compare structural elements against each other. The snapshot flags `IsStructural = true`.
> 

## 4.2 UnconnectedElementChecker.cs

**Purpose:** Finds MEP elements with open (unconnected) connector ends.

**Algorithm:**

- A connector is "open" if: `IsConnected = false` AND not an end-cap AND element is not a terminal
- **Terminal categories to exclude:** Plumbing Fixtures, Sprinklers, Air Terminals, Mechanical Equipment

**Output:** `[Category] (ID: {id}) has an open connector at {connectorDescription} on Level {level}`

## 4.3 MissingParameterChecker.cs

**Purpose:** Checks that every MEP element has required parameters filled in. Missing = null, empty string, or `<none>`.

**Required parameters by category (from config.json):**

| Category | Required parameters | Severity |
| --- | --- | --- |
| Ducts | System Name, System Classification, Insulation Type, Flow | Warning |
| Duct Fittings | System Name | Info |
| Pipes | System Name, System Classification, Pipe Material, Outside Diameter, Insulation Type | Warning |
| Pipe Fittings | System Name | Info |
| Sprinklers | System Name, Head Type, Coverage Radius, Flow | Critical |
| Mechanical Equipment | System Name, Manufacturer, Model | Warning |
| Plumbing Fixtures | System Name, Fixture Type | Warning |

**Output:** `[Category] (ID: {id}) is missing required parameter: {parameterName}`

## 4.4 PipeSlopeChecker.cs

**Purpose:** Verifies drainage/waste pipes have correct slope.

**Scope — gravity-drained systems only:**

- Include: Sanitary, Vent, Storm Drain, Domestic Cold Water Return
- Exclude: Domestic Hot Water, Domestic Cold Water Supply, Fire Protection

**Algorithm:**

- Calculate slope: `(start.Z - end.Z) / horizontal_length × 100 = slope %`
- Min slope: **1%** for pipes DN50+; **2%** for pipes below DN50
- Max slope: **15%**
- Negative slope (uphill) → Critical + "wrong direction" note
- Slope < minimum → Critical
- Slope > maximum → Warning

**Output:** `Drainage pipe (ID: {id}) on Level {level} has slope {measured}% — minimum required is {required}%`

## 4.5 SprinklerCoverageChecker.cs

**Purpose:** Checks sprinkler heads provide adequate floor coverage.

**Algorithm:**

- For each room: get floor boundary polygon
- Grid-sample the room at 0.5m resolution
- Check each sample point against nearest head coverage radius (default 2.25m)
- 
    
    > 5% of room area uncovered → **Critical**
    > 
- 1–5% uncovered → **Warning**

**Output:** `Room {roomName} on Level {level} has ~{area}m² outside sprinkler coverage. Nearest head is {distance}m from uncovered zone.`

> ⚠️ If no rooms exist on a level: create Info issue — `No rooms found on Level {level} — sprinkler coverage check skipped.`
> 

---

# 5. Revit API layer

## 5.1 RevitModelSnapshot (abstraction object)

```csharp
public class RevitModelSnapshot
{
    public string            ProjectName  { get; set; }
    public string            ModelPath    { get; set; }
    public string            RevitVersion { get; set; }
    public List<MEPElement>  Elements     { get; set; } = new();
    public List<RoomData>    Rooms        { get; set; } = new();
    public List<LevelData>   Levels       { get; set; } = new();
}

public class MEPElement
{
    public long                         Id           { get; set; }
    public string                       Category     { get; set; }
    public string                       FamilyName   { get; set; }
    public string                       Level        { get; set; }
    public BoundingBoxData              BoundingBox  { get; set; }
    public List<ConnectorData>          Connectors   { get; set; } = new();
    public Dictionary<string, string>   Parameters   { get; set; } = new();
    public bool                         IsStructural { get; set; }
    public GeometryData                 Geometry     { get; set; }
}

public class BoundingBoxData
{
    public double MinX, MinY, MinZ, MaxX, MaxY, MaxZ;
    public bool Overlaps(BoundingBoxData other) =>
        MinX <= other.MaxX && MaxX >= other.MinX &&
        MinY <= other.MaxY && MaxY >= other.MinY &&
        MinZ <= other.MaxZ && MaxZ >= other.MinZ;
}
```

## 5.2 RevitModelAdapter key rules

- Use `FilteredElementCollector` with category filters — **never** collect all and filter in memory
- Collect: `OST_DuctCurves`, `OST_DuctFitting`, `OST_PipeCurves`, `OST_PipeFitting`, `OST_Sprinklers`, `OST_PlumbingFixtures`, `OST_MechanicalEquipment`, `OST_StructuralColumns`, `OST_StructuralFraming`, `OST_Rooms`
- **Convert all units to metric** — Revit stores in feet, multiply by 0.3048 for metres
- Must run on main Revit thread. Use `ExternalEvent` if triggered from UI

## 5.3 App.cs

```csharp
[Transaction(TransactionMode.Manual)]
public class App : IExternalApplication
{
    public static App Instance  { get; private set; }
    public QCDashboardPanel DashboardPanel { get; private set; }
    public QCReport         LastReport     { get; private set; }

    public Result OnStartup(UIControlledApplication app)
    {
        Instance = this;
        RibbonSetup.CreateRibbon(app);
        DashboardPanel = new QCDashboardPanel();
        app.RegisterDockablePane(
            new DockablePaneId(new Guid("A1B2C3D4-...")),
            "MEP QC Checker",
            DashboardPanel);
        return Result.Succeeded;
    }

    public void UpdateReport(QCReport report)
    {
        LastReport = report;
        DashboardPanel.Dispatcher.Invoke(() => DashboardPanel.Bind(report));
    }
}
```

## 5.4 RunQCCheckCommand.cs

```csharp
[Transaction(TransactionMode.Manual)]
public class RunQCCheckCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData data, ref string msg, ElementSet elements)
    {
        var uiDoc = data.Application.ActiveUIDocument;
        var doc   = uiDoc.Document;

        // 1. Build snapshot (reads model, no transaction needed)
        var snapshot = new RevitModelAdapter(doc).BuildSnapshot();

        // 2. Run all checks (pure C#, no Revit API)
        var report = new CheckRunner().RunAll(snapshot);

        // 3. Apply color overrides (needs transaction)
        using (var tx = new Transaction(doc, "MEP QC - Apply Color Overrides"))
        {
            tx.Start();
            new ColorOverrideService(doc, uiDoc.ActiveView).ApplyOverrides(report);
            tx.Commit();
        }

        // 4. Push results to dashboard
        App.Instance.UpdateReport(report);
        return Result.Succeeded;
    }
}
```

---

# 6. Dashboard panel — UI specification

A **WPF DockablePane** registered with Revit. Docks right by default.

## 6.1 Panel layout

| Section | Content |
| --- | --- |
| Summary row | Three stat cards: Critical (red) · Warning (amber) · Info (gray). Updates after each scan. |
| Filter bar | Discipline dropdown · Severity dropdown |
| Issue list | Scrollable. Each row: colored dot · check type · description · element ID · level. Click → selects & zooms to element. |
| Action bar | [Run Check] · [Clear Highlights] · [Export...] (disabled in MVP, tooltip: "Phase 2") |
| Status bar | Last run: {datetime} · {total} issues · Model: {projectName} |

## 6.2 Color overrides in 3D view

| Severity | Color | Applied via |
| --- | --- | --- |
| Critical | RGB(220, 50, 50) — red | `OverrideGraphicSettings.SetSurfaceForegroundPatternColor`  • solid fill |
| Warning | RGB(230, 160, 0) — amber | Same |
| Info | No override | Panel only |

> ⚠️ If active view is 2D, switch to default 3D view. Always clear previous overrides before applying new ones.
> 

---

# 7. Ribbon tab

| Tab | Panel | Button | Command |
| --- | --- | --- | --- |
| MEP Tools | QC Checker | Run QC Check | `RunQCCheckCommand` |
| MEP Tools | QC Checker | Clear Highlights | `ClearOverridesCommand` |

32×32 icons embedded as resources. Plugin must **not crash if icons are missing** — fall back to text-only.

## 7.1 .addin manifest

```xml
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
  <AddIn Type="Application">
    <n>MEP QC Checker</n>
    <Assembly>MEPQCChecker.Revit.dll</Assembly>
    <FullClassName>MEPQCChecker.Revit.App</FullClassName>
    <ClientId>YOUR-GUID-HERE</ClientId>
    <VendorId>MEPTools</VendorId>
  </AddIn>
</RevitAddIns>
```

---

# 8. Configuration file (config.json)

Placed next to the DLL. Engineers edit without recompiling.

```json
{
  "RequiredParameters": {
    "OST_DuctCurves": [
      { "Name": "System Name",           "Severity": "Warning" },
      { "Name": "System Classification", "Severity": "Warning" },
      { "Name": "Insulation Type",        "Severity": "Info"    },
      { "Name": "Flow",                   "Severity": "Warning" }
    ],
    "OST_PipeCurves": [
      { "Name": "System Name",           "Severity": "Warning" },
      { "Name": "System Classification", "Severity": "Warning" },
      { "Name": "Pipe Material",          "Severity": "Warning" },
      { "Name": "Outside Diameter",       "Severity": "Warning" },
      { "Name": "Insulation Type",        "Severity": "Info"    }
    ],
    "OST_Sprinklers": [
      { "Name": "System Name",     "Severity": "Critical" },
      { "Name": "Head Type",       "Severity": "Critical" },
      { "Name": "Coverage Radius", "Severity": "Warning"  },
      { "Name": "Flow",            "Severity": "Warning"  }
    ]
  },
  "PipeSlope": {
    "MinSlopePctLargePipe":  1.0,
    "MinSlopePctSmallPipe":  2.0,
    "MaxSlopePct":          15.0,
    "SmallPipeThresholdMM": 50
  },
  "SprinklerCoverage": {
    "DefaultCoverageRadiusM":  2.25,
    "CriticalUncoveredPct":    5.0,
    "WarningUncoveredPct":     1.0,
    "GridSamplingResolutionM": 0.5
  },
  "GravityDrainedSystemNames": [
    "Sanitary", "Vent", "Storm Drain", "Domestic Cold Water Return"
  ]
}
```

---

# 9. Installer ([Install.ps](http://Install.ps)1)

- Detects installed Revit versions via registry
- Copies correct DLL build per version (2024 build → Revit 2020–2024; 2025 build → Revit 2025–2026)
- Creates `.addin` manifest in `%AppData%\Autodesk\Revit\Addins\{version}\`
- Copies `config.json` next to DLL
- **No admin rights required** — installs to per-user AppData

---

# 10. Build system

## .csproj for Revit 2024 build

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net48</TargetFramework>
    <AssemblyName>MEPQCChecker.Revit</AssemblyName>
    <Nullable>enable</Nullable>
    <PlatformTarget>x64</PlatformTarget>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Revit_All_Main_Versions_API_x64" Version="2024.0.0" />
    <ProjectReference Include="..\MEPQCChecker.Core\MEPQCChecker.Core.csproj" />
  </ItemGroup>
</Project>
```

## GitHub Actions CI

```yaml
name: Build all Revit versions
on:
  push:
    branches: [ main ]
jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: |
            4.8.x
            8.0.x
      - run: dotnet build src/MEPQCChecker.Revit2024 -c Release
      - run: dotnet build src/MEPQCChecker.Revit2025 -c Release
      - run: dotnet test tests/MEPQCChecker.Core.Tests
```

---

# 11. Implementation order

| Step | Area | What to build | Done when |
| --- | --- | --- | --- |
| 1 | Solution setup | .sln + 4 .csproj files + folder structure. No logic yet. | `dotnet build` succeeds on both targets |
| 2 | Data models | QCSeverity, QCIssue, QCReport, RevitModelSnapshot and all nested types | Core compiles, model constructor tests pass |
| 3 | Check classes | All 5 check classes + CheckRunner | Unit tests with synthetic snapshots pass |
| 4 | Ribbon | App.cs + RibbonSetup.cs. Stub command shows TaskDialog. | Tab and buttons appear in Revit |
| 5 | Adapter | RevitModelAdapter.cs reads live model, builds snapshot | Run on real model shows non-zero issues |
| 6 | Color overrides | ColorOverrideService.cs applies red/amber fills in 3D view | Clashing elements show red in 3D |
| 7 | Dashboard UI | QCDashboardPanel.xaml — summary cards, issue list, click-to-select | Panel docks, issues shown, clicking selects element |
| 8 | Config file | Runtime loading of config.json in MissingParameterChecker + PipeSlopeChecker | Editing config changes behaviour without recompile |
| 9 | Installer | [Install.ps](http://Install.ps)1 PowerShell script | Installs on clean Revit machine without errors |
| 10 | CI pipeline | build.yml GitHub Actions | Green CI badge on every push |

---

# 12. Unit tests

All in `MEPQCChecker.Core.Tests` using **xUnit**. Must run without Revit installed.

## ClashDetector

- `Test_TwoOverlappingDucts_ReturnsOneCriticalIssue`
- `Test_TwoNonOverlappingDucts_ReturnsNoIssues`
- `Test_StructuralElementStillCausesClash`
- `Test_SameElementNotComparedToItself` — common bug!

## PipeSlopeChecker

- `Test_FlatDrainagePipe_ReturnsCritical`
- `Test_CorrectSlope_ReturnsNoIssues`
- `Test_UpwardSlopingPipe_ReturnsCritical`
- `Test_PressurizedPipeIgnored_ReturnsNoIssues`
- `Test_ExcessiveSlope_ReturnsWarning`

## MissingParameterChecker

- `Test_MissingSystemName_ReturnsWarning`
- `Test_AllParametersPresent_ReturnsNoIssues`
- `Test_EmptyStringCountsAsMissing`
- `Test_NoneValueCountsAsMissing`

## SprinklerCoverageChecker

- `Test_RoomFullyCovered_ReturnsNoIssues`
- `Test_LargeRoomOneHead_ReturnsCritical`
- `Test_NoRooms_ReturnsInfoIssue`

---

# 13. NuGet packages

| Package | Project | Purpose |
| --- | --- | --- |
| `Revit_All_Main_Versions_API_x64` | Revit | Revit API bindings |
| `System.Text.Json` | Core | config.json parsing (add package for net48; built-in for net8) |
| `xunit`  • `xunit.runner.visualstudio` | Tests | Unit test framework |
| `Microsoft.NET.Test.Sdk` | Tests | Required for `dotnet test` |

> ⚠️ Do NOT add EPPlus or iTextSharp in MVP. Minimal dependencies = fewer DLL conflicts with Revit.
> 

---

# 14. Known risks & mitigations

| Risk | Likelihood | Mitigation |
| --- | --- | --- |
| Clash check too slow on large models | Medium | Spatial grid partitioning from Day 1. Add progress bar. Use `IExternalEventHandler` for async if needed. |
| DLL version conflicts with Revit | Low–Medium | No Newtonsoft.Json. Use System.Text.Json. Test on clean Revit install early. |
| No 3D view open when check runs | Low | Check for active 3D view. Show TaskDialog if none. Fall back to default 3D view. |
| Plugin crashes silently | Low | Try/catch in all `Execute` methods. Log to `.log` file next to DLL. Friendly error dialog with log path. |
| WPF panel calling Revit API on wrong thread | Medium | Never call Revit API from WPF handlers. Use `IExternalEventHandler`  • `ExternalEvent.Raise()` pattern. |

---

# 15. Starter prompt for Claude Code

Copy this entire block into Claude Code to begin:

```
Build a Revit add-in plugin called MEP QC Checker following this exact plan:

TECH STACK:
- C# solution with 4 projects: Core (no Revit dependency), Revit (API layer),
  Revit2024 (net48), Revit2025 (net8)
- WPF for the dockable panel UI
- xUnit for unit tests
- System.Text.Json for config parsing

START WITH STEP 1:
1. Create .sln and all 4 .csproj files with correct TargetFrameworks
2. Set up folder structure as specified
3. Add NuGet references
4. Verify dotnet build succeeds

Then proceed step by step through the plan.

KEY RULES:
- Core project: ZERO Revit API references
- All Revit API calls go through RevitModelAdapter only
- All check classes receive RevitModelSnapshot, never UIDocument or Document
- config.json must be read at runtime, not hardcoded
- Every IExternalCommand.Execute must have a top-level try/catch
- WPF panel must never call Revit API directly
- Clash detection must use spatial grid partitioning

The 5 QC checks:
1. ClashDetector — bounding box overlap, spatial grid
2. UnconnectedElementChecker — open connectors, skip terminals
3. MissingParameterChecker — required params from config.json
4. PipeSlopeChecker — gravity drainage pipes only
5. SprinklerCoverageChecker — 0.5m grid sampling vs coverage radius

After each step: run dotnet build and dotnet test before proceeding.
Ask me if anything is unclear before writing code.
```

---

*Next phase: Reports (Excel/PDF) · Automation · Standards library*