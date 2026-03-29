# Step 10: CI Pipeline (GitHub Actions)

**Status:** NOT STARTED
**Depends on:** Steps 1-3 (only needs Core + Tests to be meaningful)
**Verifiable without Revit:** YES — CI runs on GitHub-hosted runners

---

## Objective

Create a GitHub Actions workflow that builds both Revit targets and runs unit tests on every push/PR.

## Files to Create

### .github/workflows/build.yml

```yaml
name: Build & Test
on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore MEPQCChecker.sln
      - run: dotnet build src/MEPQCChecker.Revit2024/MEPQCChecker.Revit2024.csproj -c Release --no-restore
      - run: dotnet build src/MEPQCChecker.Revit2025/MEPQCChecker.Revit2025.csproj -c Release --no-restore
      - run: dotnet test tests/MEPQCChecker.Core.Tests/MEPQCChecker.Core.Tests.csproj --no-build -c Release --verbosity normal
```

**Key notes:**
- `windows-latest` already has .NET Framework 4.8 pre-installed
- Only need to install .NET 8 SDK via `setup-dotnet`
- Do NOT use `4.8.x` in dotnet-version — it's not a valid SDK version
- `--no-restore` after initial restore to avoid redundant work
- `--no-build` on test step since build already happened (may need to adjust if test builds separately)

## Acceptance Criteria

- [ ] Workflow file is valid YAML
- [ ] Triggers on push to main and PRs
- [ ] Installs .NET 8 SDK
- [ ] Builds both Revit2024 (net48) and Revit2025 (net8) targets
- [ ] Runs unit tests
- [ ] Green badge on successful push
