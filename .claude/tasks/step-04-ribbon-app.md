# Step 4: Ribbon + App Entry Point

**Status:** NOT STARTED
**Depends on:** Steps 1-3 (completed)
**Verifiable without Revit:** Build only (UI appears only in Revit)

---

## Objective

Create the Revit add-in entry point (`App.cs`) and ribbon tab with buttons so the plugin loads in Revit and shows "MEP Tools" tab.

## Files to Create

### src/MEPQCChecker.Revit/App.cs
- Implements `IExternalApplication`
- `OnStartup`: calls `RibbonSetup.CreateRibbon()`, registers dockable pane
- `OnShutdown`: cleanup
- Static `Instance` property for singleton access
- Stores `QCDashboardPanel` and `QCReport LastReport`
- `UpdateReport(QCReport)` method pushes results to dashboard via Dispatcher
- Attribute: `[Transaction(TransactionMode.Manual)]`

### src/MEPQCChecker.Revit/Ribbon/RibbonSetup.cs
- Static method `CreateRibbon(UIControlledApplication app)`
- Creates tab: "MEP Tools"
- Creates panel: "QC Checker"
- Two buttons:
  - "Run QC Check" → `RunQCCheckCommand`
  - "Clear Highlights" → `ClearOverridesCommand`
- 32x32 icons embedded as resources
- **Must NOT crash if icons missing** — fall back to text-only buttons (try/catch around icon loading)

### src/MEPQCChecker.Revit/Commands/RunQCCheckCommand.cs
- Implements `IExternalCommand`
- `[Transaction(TransactionMode.Manual)]` attribute
- Execute flow:
  1. Build snapshot via `RevitModelAdapter(doc).BuildSnapshot()`
  2. Run checks via `CheckRunner().RunAll(snapshot)`
  3. Apply color overrides in a Transaction
  4. Push report to dashboard via `App.Instance.UpdateReport(report)`
- **Top-level try/catch** with logging and friendly TaskDialog on error
- For now (stub): show `TaskDialog("QC Check", "Coming soon...")`

### src/MEPQCChecker.Revit/Commands/ClearOverridesCommand.cs
- Implements `IExternalCommand`
- `[Transaction(TransactionMode.Manual)]` attribute
- Clears all color overrides from active view
- **Top-level try/catch**
- For now (stub): show `TaskDialog("Clear", "Coming soon...")`

### installer/MEPQCChecker.addin
- XML manifest per spec Section 7.1
- Generate stable GUIDs for ClientId and DockablePaneId

## Acceptance Criteria

- [ ] `dotnet build src/MEPQCChecker.Revit2024` succeeds
- [ ] `dotnet build src/MEPQCChecker.Revit2025` succeeds
- [ ] App.cs registered as IExternalApplication
- [ ] Two IExternalCommand stubs compile
- [ ] .addin manifest XML is valid
- [ ] No icon-missing crash path (graceful fallback)
