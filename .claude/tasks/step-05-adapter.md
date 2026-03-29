# Step 5: RevitModelAdapter

**Status:** NOT STARTED
**Depends on:** Steps 1-4
**Verifiable without Revit:** Build only (runtime requires live Revit model)

---

## Objective

Create `RevitModelAdapter.cs` — the single file that bridges the Revit API to our `RevitModelSnapshot` model. This is the ONLY file in the entire solution that directly calls the Revit API for data extraction.

## Files to Create

### src/MEPQCChecker.Revit/Adapters/RevitModelAdapter.cs

**Constructor:** Takes `Autodesk.Revit.DB.Document`

**Main method:** `RevitModelSnapshot BuildSnapshot()`

### Data Extraction Rules

1. **Use FilteredElementCollector with category filters** — never collect all elements
2. **Categories to collect:**
   - `OST_DuctCurves`, `OST_DuctFitting`, `OST_DuctAccessory`
   - `OST_PipeCurves`, `OST_PipeFitting`, `OST_PipeAccessory`
   - `OST_Sprinklers`
   - `OST_PlumbingFixtures`
   - `OST_MechanicalEquipment`
   - `OST_CableTray`, `OST_Conduit`
   - `OST_PipeInsulation`, `OST_DuctInsulation`
   - `OST_StructuralColumns`, `OST_StructuralFraming` (for clash detection)
   - `OST_Rooms` (for sprinkler coverage)

3. **Unit conversion:** Revit stores in internal feet → multiply by 0.3048 for metres
   ```csharp
   private const double FeetToMetres = 0.3048;
   ```

4. **For each element, extract:**
   - Id (ElementId.IntegerValue / Value)
   - Category name (as OST_ string)
   - Family name
   - Level name
   - BoundingBox (converted to metres)
   - All connectors with IsConnected status
   - All instance + type parameters as string dictionary
   - IsStructural flag
   - Geometry data (start/end points for pipes/ducts via LocationCurve)

5. **For rooms:** Extract boundary polygon via `Room.GetBoundarySegments()`

6. **Thread safety:** Must run on Revit's main thread. If triggered from UI, use `ExternalEvent` pattern.

7. **Null handling:** Some elements have no bounding box, no connectors, or no LocationCurve — skip gracefully.

## Acceptance Criteria

- [ ] Compiles in both Revit2024 and Revit2025 build targets
- [ ] Uses FilteredElementCollector (never collects all)
- [ ] All measurements converted to metric
- [ ] Handles null bounding boxes / connectors gracefully
- [ ] Extracts rooms with boundary polygons
- [ ] On a real Revit model: produces non-zero element snapshot
