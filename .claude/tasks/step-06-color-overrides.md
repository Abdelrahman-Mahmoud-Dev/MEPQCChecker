# Step 6: Color Overrides

**Status:** NOT STARTED
**Depends on:** Steps 1-5
**Verifiable without Revit:** Build only

---

## Objective

Create `ColorOverrideService` that highlights clashing/problematic elements in the 3D view using Revit's graphic override system.

## Files to Create

### src/MEPQCChecker.Revit/Services/ColorOverrideService.cs

**Note:** This lives in the Revit layer (NOT Core) because it uses Revit API directly.

**Constructor:** Takes `Document` and `View`

### Color Scheme

| Severity | Color | RGB |
|----------|-------|-----|
| Critical | Red | (220, 50, 50) |
| Warning | Amber | (230, 160, 0) |
| Info | No override | Panel display only |

### Implementation

1. **`ClearOverrides()`** — Reset all element overrides in active view
2. **`ApplyOverrides(QCReport report)`**:
   - Clear previous overrides first
   - For each Critical issue: apply red solid fill via `OverrideGraphicSettings.SetSurfaceForegroundPatternColor`
   - For each Warning issue: apply amber solid fill
   - Info issues: no visual override
3. **View check:** If active view is 2D, find or create the default 3D view (`ViewType.ThreeD`)
4. **Must run inside a Transaction**

### Also Create (in Core)

**src/MEPQCChecker.Core/Services/HighlightPlan.cs**
```csharp
public class HighlightPlan
{
    public List<long> CriticalElementIds { get; set; }
    public List<long> WarningElementIds { get; set; }
}
```
- Static factory method: `HighlightPlan.FromReport(QCReport report)` — extracts element IDs by severity
- This keeps the logic of "which elements to highlight" in Core (testable)

## Acceptance Criteria

- [ ] Compiles in both build targets
- [ ] Clears previous overrides before applying new ones
- [ ] Critical = red fill, Warning = amber fill
- [ ] Handles 2D active view (switches to 3D)
- [ ] HighlightPlan.FromReport() unit test in Core.Tests
