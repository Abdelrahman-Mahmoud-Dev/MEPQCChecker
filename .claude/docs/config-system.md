## Config System

**Purpose:** Runtime-loadable config.json for parameter requirements, slope thresholds, and sprinkler coverage settings.

**Files Involved:**
- `config.json` — configuration file placed next to DLL
- `src/MEPQCChecker.Core/Services/ConfigService.cs` — deserializes config.json using System.Text.Json
- `src/MEPQCChecker.Core/Checks/MissingParameterChecker.cs` — consumes RequiredParameters
- `src/MEPQCChecker.Core/Checks/PipeSlopeChecker.cs` — consumes PipeSlope thresholds
- `src/MEPQCChecker.Core/Checks/SprinklerCoverageChecker.cs` — consumes SprinklerCoverage settings

**Status:** Planned

**Data Schema:**
- `RequiredParameters`: dict of category → list of {Name, Severity}
- `PipeSlope`: {MinSlopePctLargePipe, MinSlopePctSmallPipe, MaxSlopePct, SmallPipeThresholdMM}
- `SprinklerCoverage`: {DefaultCoverageRadiusM, CriticalUncoveredPct, WarningUncoveredPct, GridSamplingResolutionM}
- `GravityDrainedSystemNames`: string array

**Gotchas:**
- System.Text.Json must be added as NuGet package for netstandard2.0/net48 (built-in for net8)
- Config path resolved from: `Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)`
- Engineers can edit config.json without recompiling
- Do NOT use Newtonsoft.Json — risk of DLL conflicts with Revit
