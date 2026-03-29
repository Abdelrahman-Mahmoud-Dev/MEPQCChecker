# Step 9: Installer (Install.ps1)

**Status:** NOT STARTED
**Depends on:** Steps 1-8
**Verifiable without Revit:** Script logic review only

---

## Objective

Create a PowerShell installer script that detects installed Revit versions and copies the correct DLL build to each.

## Files to Create

### installer/Install.ps1

**Algorithm:**
1. Detect installed Revit versions by scanning registry:
   - `HKLM:\SOFTWARE\Autodesk\Revit\*`
   - Extract year from each key
2. For each detected version:
   - Determine build target:
     - 2020-2024 → use net48 build (from Revit2024 output)
     - 2025-2026 → use net8 build (from Revit2025 output)
   - Target directory: `$env:APPDATA\Autodesk\Revit\Addins\{year}\MEPQCChecker\`
   - Copy files:
     - `MEPQCChecker.Revit.dll`
     - `MEPQCChecker.Core.dll`
     - `config.json`
     - Any transitive dependency DLLs (System.Text.Json etc.)
   - Create `.addin` manifest: `$env:APPDATA\Autodesk\Revit\Addins\{year}\MEPQCChecker.addin`
3. Print summary of what was installed where

**Requirements:**
- No admin rights — everything goes to per-user %APPDATA%
- Idempotent — safe to run multiple times
- Creates directories if they don't exist
- Handles missing build output gracefully (error message, not crash)

### installer/Uninstall.ps1

- Removes all installed files and directories
- Removes .addin manifests
- Safe to run even if nothing was installed

## Acceptance Criteria

- [ ] Install.ps1 runs without errors on a clean machine
- [ ] Detects Revit versions from registry
- [ ] Copies correct build per version (net48 vs net8)
- [ ] Creates .addin manifest in correct location
- [ ] No admin rights required
- [ ] Idempotent (safe to re-run)
- [ ] Uninstall.ps1 cleans up completely
