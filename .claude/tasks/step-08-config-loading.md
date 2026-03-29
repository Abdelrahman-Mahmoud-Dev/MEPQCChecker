# Step 8: Config File Runtime Loading

**Status:** NOT STARTED
**Depends on:** Steps 1-3 (ConfigService already stubbed with defaults)
**Verifiable without Revit:** YES — fully testable

---

## Objective

Wire up `ConfigService.Load()` to actually read `config.json` from disk at runtime. Editing the JSON file should change check behavior without recompiling.

## Files to Modify

### src/MEPQCChecker.Core/Services/ConfigService.cs
- `Load()` already implemented — reads from assembly directory
- Verify it works end-to-end with the actual config.json file

### src/MEPQCChecker.Core/MEPQCChecker.Core.csproj
- Add config.json copy to output:
  ```xml
  <ItemGroup>
    <None Update="config.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
  ```

### src/MEPQCChecker.Core/Services/CheckRunner.cs
- Update default constructor to use `ConfigService.Load()` instead of `GetDefaults()`
- Add fallback: if Load() throws, use GetDefaults()

## Tests to Add

### tests/MEPQCChecker.Core.Tests/Services/ConfigServiceTests.cs
- `LoadFromJson_ValidJson_ReturnsConfig` — parse the actual config.json content
- `LoadFromJson_EmptyJson_ReturnsFallbackDefaults`
- `GetDefaults_ReturnsNonEmptyConfig` — verify default config has all expected categories
- `MissingParameterChecker_WithCustomConfig_UsesConfigValues` — verify a checker uses loaded config

## Acceptance Criteria

- [ ] config.json is copied to output directory on build
- [ ] ConfigService.Load() reads and parses config.json
- [ ] ConfigService.LoadFromJson() works with valid JSON
- [ ] Fallback to defaults if config.json missing or invalid
- [ ] New unit tests pass
- [ ] Changing a value in config.json changes checker behavior (manual verification)
