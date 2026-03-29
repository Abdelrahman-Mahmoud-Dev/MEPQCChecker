# Phase 2: Linked-File Clash Detection + Auto-Fix

## Context

The MEP QC Checker MVP detects clashes, unconnected elements, missing parameters, pipe slope violations, and sprinkler coverage gaps within a **single model**. Two highly requested Phase 2 features are:

1. **Multi-model / linked-file clash detection** -- detect clashes between host model elements and linked Revit model elements
2. **Automated fixing** -- preview proposed fixes for all check types and apply them via Revit transactions

Both features must preserve the Core/API separation (Core has zero Revit API references).

---

## Feature 1: Linked-File Clash Detection

### Scope
- Host model vs. linked Revit models only (not cross-link, not IFC/CAD)
- UI toggle per linked model (checkboxes to include/exclude)
- Linked model transforms applied to bounding boxes

### Core Model Changes

**New file: `src/MEPQCChecker.Core/Models/LinkedModelInfo.cs`**
```csharp
public class LinkedModelInfo
{
    public string ModelId { get; set; }       // RevitLinkType.UniqueId
    public string ModelName { get; set; }     // e.g. "Structure.rvt"
    public string ModelPath { get; set; }
    public bool IsIncluded { get; set; } = true;  // UI toggle state
    public int ElementCount { get; set; }
}
```

**Modify: `MEPElement.cs`** -- add source tracking
```csharp
public string? SourceModelId { get; set; }    // null = host model
public string? SourceModelName { get; set; }
```

**Modify: `RevitModelSnapshot.cs`** -- add linked model list
```csharp
public List<LinkedModelInfo> LinkedModels { get; set; } = new();
```

**Modify: `QCIssue.cs`** -- add source model fields for both elements
```csharp
public string? SourceModelId { get; set; }
public string? SourceModelName { get; set; }
public string? SourceModelId2 { get; set; }
public string? SourceModelName2 { get; set; }
```

### ClashDetector Changes (`ClashDetector.cs`)

When linked elements are present in the snapshot:
- **Remove level-grouping** (level names differ across models) -- rely on spatial grid only
- **Filter comparisons**: only compare host vs. linked elements:
  ```csharp
  bool aIsHost = string.IsNullOrEmpty(a.SourceModelId);
  bool bIsHost = string.IsNullOrEmpty(b.SourceModelId);
  if (aIsHost == bIsHost) continue; // skip host-vs-host, link-vs-link
  ```
- **Update pair key** from `(long, long)` to `(string?, long, string?, long)` for cross-model uniqueness
- **Filter by IsIncluded**: skip elements from excluded linked models
- When no linked elements exist, fall back to original same-model behavior (backward compatible)
- Populate `SourceModelId`/`SourceModelName` on QCIssue

### RevitModelAdapter Changes (`RevitModelAdapter.cs`)

- Add `BuildSnapshot(HashSet<string>? includedLinkIds = null)` overload
- After collecting host elements, discover linked models:
  ```csharp
  var linkInstances = new FilteredElementCollector(_doc)
      .OfClass(typeof(RevitLinkInstance))
      .Cast<RevitLinkInstance>()
      .Where(li => li.GetLinkDocument() != null);
  ```
- For each `RevitLinkInstance`:
  1. Get `linkDoc = linkInstance.GetLinkDocument()`
  2. Get `transform = linkInstance.GetTotalTransform()`
  3. Collect MEP + structural elements from `linkDoc`
  4. **Transform bounding boxes**: transform all 8 corners of BB, recompute AABB in host coordinates
  5. Set `SourceModelId` / `SourceModelName` on each element
  6. Add `LinkedModelInfo` to snapshot
- Fix `GetLevelName` to use `element.Document` instead of `_doc` for linked elements

### UI Changes

**Dashboard (`QCDashboardPanel.xaml` + `.cs`):**
- Add collapsible "Linked Models" section with per-link checkboxes
- Add "Source" column to issue list GridView
- Update `IssueList_SelectionChanged` to pass `SourceModelId` to zoom handler
- Checkbox toggle updates `App.Instance.IncludedLinkModelIds`

**ZoomToElementHandler:** Add `TargetSourceModelId` property. If set, find the RevitLinkInstance, get element from link doc, transform BB, zoom via `ZoomAndCenterRectangle`.

**ColorOverrideService:** Only highlight the host-side element in clash pairs (Revit API limitation: cannot override individual linked elements in most versions). Document this limitation.

**App.cs:** Add `public HashSet<string>? IncludedLinkModelIds { get; set; }`

### Tests
- `HostVsLinked_OverlappingBBoxes_ReturnsClash`
- `HostVsHost_WithLinksPresent_NoClash`
- `LinkedVsLinked_NoClash`
- `ExcludedLink_ElementsIgnored`
- `NoLinkedElements_FallsBackToOriginalBehavior`

---

## Feature 2: Automated Fixing

### Scope
- All 5 check types support auto-fix
- Preview-then-apply workflow (modal dialog with checkboxes)
- Single undo for entire batch (TransactionGroup)

### Core Model Changes

**New file: `src/MEPQCChecker.Core/Models/FixProposal.cs`**
```csharp
public enum FixActionType { MoveElement, ConnectElements, SetParameter, AdjustElevation, PlaceSprinklerHead }
public enum FixConfidence { High, Medium, Low }

public class FixProposal
{
    public string FixId { get; set; }          // GUID
    public string IssueId { get; set; }        // links to QCIssue
    public FixActionType ActionType { get; set; }
    public FixConfidence Confidence { get; set; }
    public string Description { get; set; }
    public long ElementId { get; set; }
    public long? ElementId2 { get; set; }
    public bool IsSelected { get; set; } = true;  // preview dialog state

    // MoveElement fields (metres)
    public double? MoveX { get; set; }
    public double? MoveY { get; set; }
    public double? MoveZ { get; set; }

    // SetParameter fields
    public string? ParameterName { get; set; }
    public string? ParameterValue { get; set; }

    // AdjustElevation fields
    public double? NewEndZ { get; set; }

    // ConnectElements fields
    public int? ConnectorIndex1 { get; set; }
    public int? ConnectorIndex2 { get; set; }

    // PlaceSprinklerHead fields
    public double? PlaceX { get; set; }
    public double? PlaceY { get; set; }
    public double? PlaceZ { get; set; }
    public string? FamilyTypeName { get; set; }
}
```

**New file: `src/MEPQCChecker.Core/Models/FixResult.cs`**
```csharp
public enum FixOutcome { Applied, Skipped, Failed }
public class FixResult
{
    public string FixId { get; set; }
    public FixOutcome Outcome { get; set; }
    public string Message { get; set; }
}
```

**Modify: `QCIssue.cs`** -- add fix proposals list
```csharp
public List<FixProposal> FixProposals { get; set; } = new();
```

**Modify: `ConfigService.cs`** -- add `DefaultValue` to `RequiredParameterEntry`
```csharp
public string? DefaultValue { get; set; }  // null = no auto-fix
```

### Check Modifications (fix proposal generation)

| Check | Fix Type | Algorithm | Confidence |
|-------|----------|-----------|------------|
| ClashDetector | MoveElement | Find min-overlap axis, nudge element by overlap + 1cm buffer | Medium |
| UnconnectedElementChecker | ConnectElements | Find nearest open connector within 50mm tolerance | Medium |
| MissingParameterChecker | SetParameter | Use `DefaultValue` from config (null = no fix) | High |
| PipeSlopeChecker | AdjustElevation | Calculate required endpoint Z for minimum slope | Medium |
| SprinklerCoverageChecker | PlaceSprinklerHead | Centroid of uncovered sample points | Low |

**Clash nudge algorithm:**
1. Compute overlap on each axis (X, Y, Z)
2. Pick the axis with smallest overlap (cheapest separation)
3. Direction: away from other element's center
4. Amount: overlap + 0.01m buffer
5. Always move the MEP element (not structural)

**Pipe slope fix:** Only for too-flat pipes (not wrong-direction -- those need manual rerouting). Calculate `requiredDz = minSlope/100 * horizontalLength`, lower downstream endpoint.

### Revit Layer

**New file: `src/MEPQCChecker.Revit/Services/FixExecutor.cs`**
- Takes `Document`, has `Apply(FixProposal) -> FixResult`
- Switch on `ActionType`:
  - `MoveElement`: `ElementTransformUtils.MoveElement(doc, id, XYZ(dx, dy, dz))`
  - `SetParameter`: `element.LookupParameter(name).Set(value)`
  - `AdjustElevation`: modify `LocationCurve.Curve` with new endpoint Z
  - `ConnectElements`: align connectors, call `connector1.ConnectTo(connector2)`
  - `PlaceSprinklerHead`: `doc.Create.NewFamilyInstance(point, symbol, level, NonStructural)`
- Does NOT manage transactions (caller does)

**New file: `src/MEPQCChecker.Revit/UI/ApplyFixHandler.cs`**
- `IExternalEventHandler` implementation
- Wraps all fixes in `TransactionGroup` with per-fix `Transaction`
- Each fix commits or rolls back independently
- `TransactionGroup.Assimilate()` = single Ctrl+Z undo
- Fires `FixesApplied` event with results

### Preview Dialog

**New files: `src/MEPQCChecker.Revit/UI/FixPreviewDialog.xaml` + `.cs`**
- Modal WPF window with DataGrid
- Columns: checkbox, type, description, confidence (color-coded), element ID
- Select All / Deselect All / Apply Selected / Cancel buttons
- Confidence legend: High (green), Medium (amber), Low (orange)

### Dashboard Integration

- Add "Fix Issues..." button in action bar (enabled when report has fixable issues)
- Click handler: collect all `FixProposals` from report, show `FixPreviewDialog`, on confirm raise `ApplyFixEvent`
- Register `ApplyFixHandler` in `App.cs OnStartup`

**New file: `src/MEPQCChecker.Revit/UI/Converters/ConfidenceToColorConverter.cs`**

### Tests
- Each checker: verify fix proposals are generated with correct values
- ClashDetector: nudge vector direction and magnitude
- MissingParameterChecker: proposals only when DefaultValue is non-null
- PipeSlopeChecker: correct NewEndZ calculation
- FixProposal/FixResult model tests

---

## Implementation Order

### Phase A: Foundation (both features share these)
1. `FixProposal.cs`, `FixResult.cs` (Core/Models)
2. `LinkedModelInfo.cs` (Core/Models)
3. `MEPElement.cs` additions (SourceModelId/Name)
4. `QCIssue.cs` additions (SourceModel fields + FixProposals)
5. `RevitModelSnapshot.cs` additions (LinkedModels)

### Phase B: Linked-File Clash Detection
6. `ClashDetector.cs` -- cross-model comparison logic
7. `RevitModelAdapter.cs` -- linked model collection + transform
8. `HighlightPlan.cs` -- linked element refs
9. `App.cs` -- IncludedLinkModelIds
10. `QCDashboardPanel.xaml/.cs` -- linked models section + source column
11. `ZoomToElementHandler.cs` -- linked element zoom
12. Unit tests for linked clash detection

### Phase C: Auto-Fix (start with simplest, layer up)
13. `ConfigService.cs` -- DefaultValue on RequiredParameterEntry
14. `MissingParameterChecker.cs` -- SetParameter proposals
15. `FixExecutor.cs` -- ApplySetParameter method
16. `ApplyFixHandler.cs` -- IExternalEventHandler
17. `FixPreviewDialog.xaml/.cs` -- preview UI
18. `QCDashboardPanel` -- Fix Issues button + wiring
19. `App.cs` -- register ApplyFixHandler
20. `ClashDetector.cs` -- nudge proposals + FixExecutor.ApplyMove
21. `PipeSlopeChecker.cs` -- elevation proposals + FixExecutor.ApplyAdjustElevation
22. `UnconnectedElementChecker.cs` -- connect proposals + FixExecutor.ApplyConnect
23. `SprinklerCoverageChecker.cs` -- placement proposals + FixExecutor.ApplyPlaceSprinkler
24. Unit tests for all fix proposals

### Phase D: Polish
25. `ConfidenceToColorConverter.cs`
26. Re-run QC check after fixes applied (refresh dashboard)
27. Feature spec docs update

---

## Verification Plan

1. **Build**: `dotnet build MEPQCChecker.sln -c Release` -- all targets pass
2. **Unit tests**: `dotnet test tests/MEPQCChecker.Core.Tests` -- all pass including new tests
3. **Linked clash detection** (in Revit):
   - Open a host model with at least one linked Revit model
   - Run QC Check -- linked models appear in dashboard with checkboxes
   - Verify clashes between host ducts/pipes and linked elements are detected
   - Uncheck a link, re-run -- its elements are excluded
   - Click a linked-model issue -- zooms to correct location
4. **Auto-fix** (in Revit):
   - Run QC Check on a model with known issues
   - Click "Fix Issues..." -- preview dialog shows proposals with confidence levels
   - Deselect a few, click "Apply Selected"
   - Verify elements modified correctly (parameter filled, element moved, etc.)
   - Ctrl+Z -- all fixes undo as a single step
   - Re-run QC Check -- fixed issues no longer appear

---

## Files Summary

| File | Action | Feature |
|------|--------|---------|
| `Core/Models/LinkedModelInfo.cs` | NEW | Linked |
| `Core/Models/FixProposal.cs` | NEW | AutoFix |
| `Core/Models/FixResult.cs` | NEW | AutoFix |
| `Core/Models/MEPElement.cs` | MODIFY | Linked |
| `Core/Models/QCIssue.cs` | MODIFY | Both |
| `Core/Models/RevitModelSnapshot.cs` | MODIFY | Linked |
| `Core/Checks/ClashDetector.cs` | MODIFY | Both |
| `Core/Checks/UnconnectedElementChecker.cs` | MODIFY | AutoFix |
| `Core/Checks/MissingParameterChecker.cs` | MODIFY | AutoFix |
| `Core/Checks/PipeSlopeChecker.cs` | MODIFY | AutoFix |
| `Core/Checks/SprinklerCoverageChecker.cs` | MODIFY | AutoFix |
| `Core/Services/ConfigService.cs` | MODIFY | AutoFix |
| `Core/config.json` | MODIFY | AutoFix |
| `Revit/Adapters/RevitModelAdapter.cs` | MODIFY | Linked |
| `Revit/Services/FixExecutor.cs` | NEW | AutoFix |
| `Revit/UI/ApplyFixHandler.cs` | NEW | AutoFix |
| `Revit/UI/FixPreviewDialog.xaml` | NEW | AutoFix |
| `Revit/UI/FixPreviewDialog.xaml.cs` | NEW | AutoFix |
| `Revit/UI/Converters/ConfidenceToColorConverter.cs` | NEW | AutoFix |
| `Revit/UI/QCDashboardPanel.xaml` | MODIFY | Both |
| `Revit/UI/QCDashboardPanel.xaml.cs` | MODIFY | Both |
| `Revit/UI/ZoomToElementHandler.cs` | MODIFY | Linked |
| `Revit/Services/HighlightPlan.cs` | MODIFY | Linked |
| `Revit/App.cs` | MODIFY | Both |
| `Tests/Checks/ClashDetectorTests.cs` | MODIFY | Both |
| `Tests/Checks/MissingParameterCheckerTests.cs` | MODIFY | AutoFix |
| `Tests/Checks/PipeSlopeFixTests.cs` | NEW | AutoFix |
| `Tests/Models/FixProposalTests.cs` | NEW | AutoFix |

## Known Limitations
- **Cannot color individual linked elements** in Revit API (most versions) -- only host-side element gets highlighted
- **Cannot select individual linked sub-elements** programmatically -- zoom uses BB rectangle instead
- **Clash nudge is approximate** -- moves by min-overlap axis, may create new clashes elsewhere
- **Sprinkler placement** is Low confidence -- requires correct family type loaded in model
- **Wrong-direction pipes** get no auto-fix -- too risky, needs manual rerouting
