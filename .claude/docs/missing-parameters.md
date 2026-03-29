## Missing Parameter Check

**Purpose:** Checks that every MEP element has required parameters filled in. Missing = null, empty string, or `<none>`.

**Files Involved:**
- `src/MEPQCChecker.Core/Checks/MissingParameterChecker.cs` — IQCCheck implementation
- `src/MEPQCChecker.Core/Services/ConfigService.cs` — loads required params from config.json
- `config.json` — runtime configuration
- `tests/MEPQCChecker.Core.Tests/Checks/MissingParameterCheckerTests.cs` — unit tests

**Status:** Planned

**Data Flow:**
1. Load required parameter definitions from config.json (category → parameter list with severity)
2. For each element, look up its category in the config
3. For each required parameter, check if it exists in element's Parameters dictionary
4. Values of null, empty string, or `<none>` count as missing
5. Emit `QCIssue` with severity from config for each missing parameter

**Gotchas:**
- Parameter names must match exactly (case-sensitive)
- Config is loaded at runtime — editing config.json changes behavior without recompile
- Different categories have different severity levels for the same check type
