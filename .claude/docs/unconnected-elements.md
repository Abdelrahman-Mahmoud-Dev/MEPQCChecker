## Unconnected Element Check

**Purpose:** Finds MEP elements with open (unconnected) connector ends.

**Files Involved:**
- `src/MEPQCChecker.Core/Checks/UnconnectedElementChecker.cs` — IQCCheck implementation
- `tests/MEPQCChecker.Core.Tests/Checks/UnconnectedElementCheckerTests.cs` — unit tests

**Status:** Planned

**Data Flow:**
1. Iterate all elements in snapshot
2. For each element's connectors, check `IsConnected` property
3. A connector is "open" if: `IsConnected = false` AND not an end-cap AND element is not a terminal
4. Terminal categories to exclude: Plumbing Fixtures, Sprinklers, Air Terminals, Mechanical Equipment
5. Emit `QCIssue` with `Severity = Warning` for each open connector

**Gotchas:**
- Terminal elements (fixtures, sprinklers, air terminals, mech equipment) have intentionally unconnected ends
- End-caps also should not be flagged
