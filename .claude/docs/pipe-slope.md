## Pipe Slope Check

**Purpose:** Verifies drainage/waste pipes have correct slope. Only applies to gravity-drained systems.

**Files Involved:**
- `src/MEPQCChecker.Core/Checks/PipeSlopeChecker.cs` — IQCCheck implementation
- `src/MEPQCChecker.Core/Services/ConfigService.cs` — slope thresholds from config.json
- `tests/MEPQCChecker.Core.Tests/Checks/PipeSlopeCheckerTests.cs` — unit tests

**Status:** Planned

**Data Flow:**
1. Filter elements to pipes only (category = Pipes)
2. Filter to gravity-drained systems: Sanitary, Vent, Storm Drain, Domestic Cold Water Return
3. Calculate slope: `(start.Z - end.Z) / horizontal_length × 100 = slope %`
4. Check against thresholds from config:
   - Min slope: 1% for pipes DN50+; 2% for pipes below DN50
   - Max slope: 15%
5. Negative slope (uphill) → Critical + "wrong direction" note
6. Slope < minimum → Critical
7. Slope > maximum → Warning

**Gotchas:**
- Horizontal length = sqrt((endX-startX)^2 + (endY-startY)^2), NOT 3D length
- Pipe diameter determines which minimum slope applies (50mm threshold)
- Must exclude pressurized systems (Hot Water, Cold Water Supply, Fire Protection)
- System names come from config.json `GravityDrainedSystemNames` array
