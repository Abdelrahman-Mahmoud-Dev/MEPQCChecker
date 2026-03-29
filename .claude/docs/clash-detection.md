## Clash Detection

**Purpose:** Finds intersecting bounding boxes between MEP elements using spatial grid partitioning.

**Files Involved:**
- `src/MEPQCChecker.Core/Checks/ClashDetector.cs` — IQCCheck implementation
- `src/MEPQCChecker.Core/Models/BoundingBoxData.cs` — Overlaps() method
- `tests/MEPQCChecker.Core.Tests/Checks/ClashDetectorTests.cs` — unit tests

**Status:** Planned

**Data Flow:**
1. Receive `RevitModelSnapshot` with all MEP elements and bounding boxes
2. Group elements by level, then by 2m x 2m XY grid cells
3. For each cell, compare elements in same + adjacent cells (3x3 neighborhood)
4. Check bounding box overlap in all 3 axes (X, Y, Z)
5. Skip self-comparison, duplicate pairs, and structural-vs-structural
6. Emit one `QCIssue` per clash pair with `Severity = Critical`

**Gotchas:**
- Must use spatial grid partitioning — naive O(n^2) too slow on large models
- Skip structural elements (`IsStructural = true`) comparing against each other
- Track reported pairs using sorted tuple of ElementIds to avoid duplicates
- Both ElementId and ElementId2 must be populated on clash issues
