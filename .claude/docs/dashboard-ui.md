## Dashboard UI

**Purpose:** WPF dockable pane inside Revit showing QC results with filtering and click-to-zoom.

**Files Involved:**
- `src/MEPQCChecker.Revit/UI/QCDashboardPanel.xaml` — WPF layout
- `src/MEPQCChecker.Revit/UI/QCDashboardPanel.xaml.cs` — code-behind
- `src/MEPQCChecker.Revit/UI/ZoomToElementHandler.cs` — IExternalEventHandler for thread-safe zoom
- `src/MEPQCChecker.Revit/App.cs` — registers dockable pane

**Status:** Planned

**Layout:**
| Section | Content |
|---------|---------|
| Summary row | Three stat cards: Critical (red), Warning (amber), Info (gray) |
| Filter bar | Discipline dropdown, Severity dropdown |
| Issue list | Scrollable list with colored dot, check type, description, element ID, level |
| Action bar | [Run Check], [Clear Highlights], [Export...] (disabled in MVP) |
| Status bar | Last run timestamp, total issues, model name |

**Gotchas:**
- WPF runs on UI thread; Revit API must run on Revit's main thread
- Click-to-zoom must use IExternalEventHandler + ExternalEvent.Raise() pattern
- Never call Revit API directly from WPF event handlers
- Panel docks right by default
