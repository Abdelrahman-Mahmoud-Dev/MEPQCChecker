# Step 7: Dashboard UI

**Status:** NOT STARTED
**Depends on:** Steps 1-6
**Verifiable without Revit:** XAML preview only (full UI requires Revit docking)

---

## Objective

Create the WPF dockable panel that displays QC results inside Revit with filtering and click-to-zoom.

## Files to Create

### src/MEPQCChecker.Revit/UI/QCDashboardPanel.xaml + .xaml.cs
- Implements `IDockablePaneProvider` (for Revit docking)
- Registered in `App.OnStartup()` with `RegisterDockablePane()`
- Docks right by default

**Layout sections:**

| Section | Content |
|---------|---------|
| Summary row | 3 stat cards: Critical (red bg), Warning (amber bg), Info (gray bg) |
| Filter bar | Discipline dropdown (All/Mechanical/Plumbing/FireProtection), Severity dropdown (All/Critical/Warning/Info) |
| Issue list | Scrollable ListView. Each row: colored severity dot, check type, description, element ID, level |
| Action bar | [Run Check] button, [Clear Highlights] button, [Export...] button (disabled, tooltip "Phase 2") |
| Status bar | "Last run: {datetime} · {total} issues · Model: {projectName}" |

**Data binding:**
- `Bind(QCReport report)` method called from `App.UpdateReport()`
- Summary cards bind to CriticalCount, WarningCount, InfoCount
- Issue list uses CollectionViewSource for filtering
- Filter dropdowns update CollectionViewSource filter predicate

### src/MEPQCChecker.Revit/UI/ZoomToElementHandler.cs
- Implements `IExternalEventHandler`
- Stores target ElementId in a shared field
- `Execute()` runs on Revit thread: calls `UIDocument.ShowElements(elementId)` to zoom
- Created in `App.OnStartup()`, stored on `App.Instance`

**Click-to-zoom flow:**
1. User clicks issue row in ListView
2. WPF click handler sets `ZoomToElementHandler.TargetElementId`
3. Calls `App.Instance.ZoomEvent.Raise()`
4. Revit calls `ZoomToElementHandler.Execute()` on main thread
5. Handler zooms to element via `UIDocument.ShowElements()`

### src/MEPQCChecker.Revit/UI/Converters/SeverityToColorConverter.cs
- IValueConverter: QCSeverity → SolidColorBrush
- Critical → Red, Warning → Amber, Info → Gray

## Critical Thread Safety Rules

- WPF panel runs on UI thread
- Revit API MUST run on Revit's main thread
- **NEVER call Revit API from WPF event handlers directly**
- Always use `IExternalEventHandler` + `ExternalEvent.Raise()` pattern
- Run Check button → raises ExternalEvent for RunQCCheckCommand
- Click issue → raises ExternalEvent for ZoomToElementHandler

## Acceptance Criteria

- [ ] XAML compiles in both build targets
- [ ] Panel implements IDockablePaneProvider
- [ ] Summary cards show issue counts
- [ ] Filter dropdowns filter the issue list
- [ ] Click-to-zoom uses ExternalEvent pattern (no direct Revit API in WPF)
- [ ] Run Check / Clear Highlights buttons work
- [ ] Export button is disabled with "Phase 2" tooltip
- [ ] Status bar shows last run info
