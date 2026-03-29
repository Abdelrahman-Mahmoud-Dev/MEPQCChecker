## Sprinkler Coverage Check

**Purpose:** Checks sprinkler heads provide adequate floor coverage using grid sampling.

**Files Involved:**
- `src/MEPQCChecker.Core/Checks/SprinklerCoverageChecker.cs` — IQCCheck implementation
- `src/MEPQCChecker.Core/Models/RoomData.cs` — room boundary polygon
- `tests/MEPQCChecker.Core.Tests/Checks/SprinklerCoverageCheckerTests.cs` — unit tests

**Status:** Planned

**Data Flow:**
1. For each room in snapshot: get floor boundary polygon from BoundaryPoints
2. Grid-sample the room at 0.5m resolution
3. For each sample point, use point-in-polygon test (ray casting)
4. Check each interior point against nearest sprinkler head coverage radius (default 2.25m)
5. Calculate uncovered percentage:
   - >5% uncovered → Critical
   - 1-5% uncovered → Warning
6. If no rooms exist on a level → Info issue ("No rooms found, check skipped")

**Gotchas:**
- Must implement point-in-polygon (ray casting algorithm)
- Grid sampling resolution and coverage radius come from config.json
- Rooms without boundary data should be skipped with an Info issue
- Coverage radius is per-head if available, otherwise use default from config
