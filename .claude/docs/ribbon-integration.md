## Ribbon Integration

**Purpose:** Adds "MEP Tools" tab with "QC Checker" panel containing Run and Clear buttons.

**Files Involved:**
- `src/MEPQCChecker.Revit/Ribbon/RibbonSetup.cs` — creates tab and buttons
- `src/MEPQCChecker.Revit/App.cs` — calls RibbonSetup.CreateRibbon()
- `src/MEPQCChecker.Revit/Commands/RunQCCheckCommand.cs` — Run QC Check button handler
- `src/MEPQCChecker.Revit/Commands/ClearOverridesCommand.cs` — Clear Highlights button handler
- `installer/MEPQCChecker.addin` — add-in manifest XML

**Status:** Planned

**Gotchas:**
- 32x32 icons embedded as resources; must NOT crash if icons missing (fall back to text-only)
- Both commands need [Transaction(TransactionMode.Manual)] attribute
- Every Execute() method must have top-level try/catch with logging
