# Governance, Organization & Testing Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add repo governance, a tidy layout, central build config, CI, community docs, extended mutation testing, a BenchmarkDotNet perf project, and the BMAD methodology to the IndQuestEnums package repo — without changing any production behavior.

**Architecture:** Mostly additive tooling around an already-mature two-package .NET 10 NuGet library. The one behavior-adjacent change is hoisting build/package properties into `Directory.Build.props` + `Directory.Packages.props` (Central Package Management); validated by a full build/test/pack before proceeding. No `EnumModel`/converter source is touched.

**Tech Stack:** .NET 10 (`net10.0`), xUnit v3 + Shouldly (MTP runner), Stryker.NET 4.14.2, BenchmarkDotNet, GitHub Actions, BMAD-METHOD (npx).

## Global Constraints

- Target framework: `net10.0`. `Nullable=enable`, `ImplicitUsings=enable`, `LangVersion=latest` on all projects.
- Core `IndQuestEnums` project stays **dependency-free** — never add a `PackageReference` to it (ADR-0001 invariant #1).
- Core and EF companion publish at the **same version**, now from a single source (`Directory.Build.props`). Current version: `1.1.0`.
- Company/legal name everywhere: `Exxerpro Solutions SA de CV`. Copyright: `Copyright (c) 2026 Exxerpro Solutions SA de CV`.
- Project/homepage URL everywhere: `https://www.exxerpro.com` (with `www`).
- Security contact: `abel.briones@exxerpro.com`.
- Exxerpro mention must be a **non-commercial credit line**, not marketing copy.
- Stryker: `test-runner` must be `mtp` (xUnit v3 / Microsoft.Testing.Platform). `break: 0` (report-only, never fails build).
- Run all commands from the repo root `E:\Dynamic\IndFusion\IndQuestEnums` in PowerShell unless noted.
- Verification baseline command set: `dotnet build src/IndQuestEnums.sln -c Release` then `dotnet test src/IndQuestEnums.sln`.

---

### Task 1: Reorganize — move ADRs into `docs/architecture/adr/`

**Files:**
- Move: `ADR-0001.md` → `docs/architecture/adr/ADR-0001.md`
- Move: `ADR-0002.md` → `docs/architecture/adr/ADR-0002.md`
- Modify: `AGENTS.md` (ADR references + repo layout block)
- Modify: `README.md` (ADR references)

**Interfaces:**
- Produces: canonical ADR location `docs/architecture/adr/` referenced by later docs (CONTRIBUTING in Task 6).

- [ ] **Step 1: Move the ADRs with history preserved**

```bash
mkdir -p docs/architecture/adr
git mv ADR-0001.md docs/architecture/adr/ADR-0001.md
git mv ADR-0002.md docs/architecture/adr/ADR-0002.md
```

- [ ] **Step 2: Find every reference to the old ADR paths**

```bash
grep -rn "ADR-0001\|ADR-0002" AGENTS.md README.md
```
Expected: hits in AGENTS.md (the "Decision record" line, the repo-layout tree, and ADR-0002 cross-links) and possibly README.md.

- [ ] **Step 3: Update references in `AGENTS.md`**

Update the `**Decision record:**` line near the top to point at the new path, and update the repository-layout tree. Replace the line:
```
- **Decision record:** `ADR-0001.md` (authoritative copy lives in the EMIP repo at
```
with:
```
- **Decision record:** `docs/architecture/adr/ADR-0001.md` (authoritative copy lives in the EMIP repo at
```
In the "Repository layout" tree, replace the `├── ADR-0001.md` line with a `docs/` subtree:
```
├── docs/
│   └── architecture/adr/      # ADR-0001.md, ADR-0002.md
```

- [ ] **Step 4: Update references in `README.md`**

For each `ADR-0001.md` / `ADR-0002.md` link found in Step 2, prefix the path with `docs/architecture/adr/`. If README uses a bare filename link like `[ADR-0001](ADR-0001.md)`, change to `[ADR-0001](docs/architecture/adr/ADR-0001.md)`.

- [ ] **Step 5: Verify ADR-0002 internal self-link still resolves**

```bash
grep -n "ADR-0001" docs/architecture/adr/ADR-0002.md
```
ADR-0002 links to `[ADR-0001](ADR-0001.md)` — a sibling link, still correct since both moved together. No change needed; just confirm it points to the sibling filename.

- [ ] **Step 6: Build still green (docs-only change, sanity check)**

Run: `dotnet build src/IndQuestEnums.sln -c Release`
Expected: Build succeeded, 0 errors.

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "docs: move ADRs to docs/architecture/adr and update references"
```

---

### Task 2: Central build config (`Directory.Build.props` + CPM)

**Files:**
- Create: `src/Directory.Build.props`
- Create: `src/Directory.Packages.props`
- Modify: `src/IndQuestEnums/IndQuestEnums.csproj`
- Modify: `src/IndQuestEnums.EntityFramework/IndQuestEnums.EntityFramework.csproj`
- Modify: `src/IndQuestEnums.Tests/IndQuestEnums.Tests.csproj`

**Interfaces:**
- Produces: centrally-managed package versions (later Benchmarks task in Task 3 references packages **without** a `Version=` attribute) and a single `<Version>` source.

- [ ] **Step 1: Create `src/Directory.Build.props`**

```xml
<Project>

  <!-- Universal build settings for every project under src/ -->
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>

  <!-- Shared package/company metadata (harmless on non-packable projects) -->
  <PropertyGroup>
    <Version>1.1.0</Version>
    <Authors>Abel Briones; Exxerpro Solutions SA de CV</Authors>
    <Company>Exxerpro Solutions SA de CV</Company>
    <Product>IndQuestEnums</Product>
    <Copyright>Copyright (c) 2026 Exxerpro Solutions SA de CV</Copyright>
    <PackageProjectUrl>https://www.exxerpro.com</PackageProjectUrl>
    <RepositoryUrl>https://www.exxerpro.com</RepositoryUrl>
    <PublishRepositoryUrl>true</PublishRepositoryUrl>
  </PropertyGroup>

</Project>
```

- [ ] **Step 2: Create `src/Directory.Packages.props` (Central Package Management)**

```xml
<Project>

  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.0.9" />
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="18.6.0" />
    <PackageVersion Include="xunit.v3" Version="4.0.0-pre.128" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="4.0.0-pre.4" />
    <PackageVersion Include="Shouldly" Version="4.3.0" />
    <PackageVersion Include="BenchmarkDotNet" Version="0.14.0" />
  </ItemGroup>

</Project>
```

- [ ] **Step 3: Slim `src/IndQuestEnums/IndQuestEnums.csproj`**

Remove the now-centralized properties (`TargetFramework`, `ImplicitUsings`, `Nullable`, `LangVersion`, `Version`, `Authors`, `Company`, `Copyright`, `PublishRepositoryUrl`). Keep package-specific metadata. Result:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>

    <!-- NuGet Package Metadata (package-specific) -->
    <PackageId>IndQuestEnums</PackageId>
    <Description>Owned NuGet package providing the house SmartEnum base (EnumModel).</Description>
    <PackageTags>smartenum;enum;enummodel;indquest</PackageTags>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <PackageLicenseFile>LICENSE.txt</PackageLicenseFile>
    <PackageIcon>Icon.png</PackageIcon>
    <IncludeSymbols>true</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>
  </PropertyGroup>

  <ItemGroup>
    <None Include="..\..\README.md" Pack="true" PackagePath="\" />
    <None Include="..\..\LICENSE.txt" Pack="true" PackagePath="\" />
    <None Include="..\Icon.png" Pack="true" PackagePath="\" />
  </ItemGroup>

</Project>
```

- [ ] **Step 4: Slim `src/IndQuestEnums.EntityFramework/IndQuestEnums.EntityFramework.csproj`**

Remove centralized properties and strip the `Version=` from the EF package reference (now central). Result:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>

    <!-- NuGet Package Metadata (package-specific) -->
    <PackageId>IndQuestEnums.EntityFramework</PackageId>
    <Description>EF Core companion for IndQuestEnums providing ValueConverter and ValueComparer.</Description>
    <PackageTags>smartenum;enum;enummodel;efcore;entityframework;indquest</PackageTags>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <PackageLicenseFile>LICENSE.txt</PackageLicenseFile>
    <PackageIcon>Icon.png</PackageIcon>
    <IncludeSymbols>true</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\IndQuestEnums\IndQuestEnums.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" />
  </ItemGroup>

  <ItemGroup>
    <None Include="..\..\README.md" Pack="true" PackagePath="\" />
    <None Include="..\..\LICENSE.txt" Pack="true" PackagePath="\" />
    <None Include="..\Icon.png" Pack="true" PackagePath="\" />
  </ItemGroup>

</Project>
```

- [ ] **Step 5: Slim `src/IndQuestEnums.Tests/IndQuestEnums.Tests.csproj`**

Remove centralized build properties (keep `IsPackable=false`) and strip every `Version=` from package references. Result:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit.v3" />
    <PackageReference Include="xunit.runner.visualstudio">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Shouldly" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\IndQuestEnums\IndQuestEnums.csproj" />
    <ProjectReference Include="..\IndQuestEnums.EntityFramework\IndQuestEnums.EntityFramework.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 6: Restore and verify CPM resolves**

Run: `dotnet restore src/IndQuestEnums.sln`
Expected: Restore succeeds with no `NU1008` (CPM violation: stray `Version=`) errors. If `NU1008` appears, a `Version=` attribute was missed — remove it.

- [ ] **Step 7: Full build + test (regression gate)**

Run: `dotnet build src/IndQuestEnums.sln -c Release`
Then: `dotnet test src/IndQuestEnums.sln`
Expected: Build succeeded, all tests pass — identical to before the refactor.

- [ ] **Step 8: Pack and confirm unified version**

```bash
dotnet pack src/IndQuestEnums/IndQuestEnums.csproj -c Release -o src/nupkg
dotnet pack src/IndQuestEnums.EntityFramework/IndQuestEnums.EntityFramework.csproj -c Release -o src/nupkg
ls src/nupkg/*1.1.0*.nupkg
```
Expected: both `IndQuestEnums.1.1.0.nupkg` and `IndQuestEnums.EntityFramework.1.1.0.nupkg` produced (same version from the single `Directory.Build.props` source).

- [ ] **Step 9: Commit**

```bash
git add src/Directory.Build.props src/Directory.Packages.props src/IndQuestEnums/IndQuestEnums.csproj src/IndQuestEnums.EntityFramework/IndQuestEnums.EntityFramework.csproj src/IndQuestEnums.Tests/IndQuestEnums.Tests.csproj
git commit -m "build: centralize build props and adopt Central Package Management"
```

---

### Task 3: BenchmarkDotNet performance project

**Files:**
- Create: `src/IndQuestEnums.Benchmarks/IndQuestEnums.Benchmarks.csproj`
- Create: `src/IndQuestEnums.Benchmarks/BenchEnum.cs`
- Create: `src/IndQuestEnums.Benchmarks/LookupBenchmarks.cs`
- Create: `src/IndQuestEnums.Benchmarks/ConverterBenchmarks.cs`
- Create: `src/IndQuestEnums.Benchmarks/Program.cs`
- Modify: `src/IndQuestEnums.sln` (add project via `dotnet sln add`)

**Interfaces:**
- Consumes: `EnumModel.FromValue<TEnum>(int)`, `FromName<TEnum>(string)`, `FromDisplayName<TEnum>(string)`, implicit `EnumModel→int` / `EnumModel→string`, `EnumModelConverter<TEnum>` (EF), `EnumModelJsonConverter` (core), `BenchmarkDotNet` (central version).
- Produces: runnable benchmark harness; not referenced by other tasks.

- [ ] **Step 1: Create the project file**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="BenchmarkDotNet" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\IndQuestEnums\IndQuestEnums.csproj" />
    <ProjectReference Include="..\IndQuestEnums.EntityFramework\IndQuestEnums.EntityFramework.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Create a representative benchmark enum (`BenchEnum.cs`)**

```csharp
using IndQuestEnums;

namespace IndQuestEnums.Benchmarks;

/// <summary>Representative SmartEnum used to exercise the hot lookup/convert paths.</summary>
public sealed class BenchEnum : EnumModel
{
    public static readonly BenchEnum Invalid = new(EnumModel.InvalidState, EnumModel.InvalidName);
    public static readonly BenchEnum None = new(0, "None", "No State");
    public static readonly BenchEnum Started = new(1, "Started", "Started State");
    public static readonly BenchEnum Running = new(2, "Running", "Running State");
    public static readonly BenchEnum Done = new(4, "Done", "Done State");

    public BenchEnum() { }

    private BenchEnum(int value, string name, string displayName = "")
        : base(value, name, displayName)
    {
    }
}
```

- [ ] **Step 3: Create lookup/conversion benchmarks (`LookupBenchmarks.cs`)**

```csharp
using BenchmarkDotNet.Attributes;
using IndQuestEnums;

namespace IndQuestEnums.Benchmarks;

/// <summary>Benchmarks the cached value lookup, the linear name/display-name scans,
/// and the implicit conversions — the per-request hot paths from ADR-0002.</summary>
[MemoryDiagnoser]
public class LookupBenchmarks
{
    [GlobalSetup]
    public void Warm() => EnumModel.Warm<BenchEnum>();

    [Benchmark(Baseline = true)]
    public BenchEnum FromValue_CacheHit() => EnumModel.FromValue<BenchEnum>(2);

    [Benchmark]
    public BenchEnum FromName_LinearScan() => EnumModel.FromName<BenchEnum>("Running");

    [Benchmark]
    public BenchEnum FromDisplayName_LinearScan() => EnumModel.FromDisplayName<BenchEnum>("Running State");

    [Benchmark]
    public int ImplicitToInt() => BenchEnum.Running;

    [Benchmark]
    public string ImplicitToString() => BenchEnum.Running;
}
```

- [ ] **Step 4: Create converter benchmarks (`ConverterBenchmarks.cs`)**

```csharp
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using IndQuestEnums;
using IndQuestEnums.EntityFramework;

namespace IndQuestEnums.Benchmarks;

/// <summary>Benchmarks the EF int⇄enum ValueConverter and the System.Text.Json
/// converter round-trips — invoked ~8×/request per ADR-0002.</summary>
[MemoryDiagnoser]
public class ConverterBenchmarks
{
    private readonly EnumModelConverter<BenchEnum> _ef = new();
    private readonly JsonSerializerOptions _json = new();
    private string _payload = "2";

    [GlobalSetup]
    public void Setup()
    {
        EnumModel.Warm<BenchEnum>();
        _json.Converters.Add(new EnumModelJsonConverter());
        _payload = JsonSerializer.Serialize(BenchEnum.Running, _json);
    }

    [Benchmark]
    public object? Ef_ToProvider() => _ef.ConvertToProvider(BenchEnum.Running);

    [Benchmark]
    public object? Ef_FromProvider() => _ef.ConvertFromProvider(2);

    [Benchmark]
    public string Json_Serialize() => JsonSerializer.Serialize(BenchEnum.Running, _json);

    [Benchmark]
    public BenchEnum? Json_Deserialize() => JsonSerializer.Deserialize<BenchEnum>(_payload, _json);
}
```

- [ ] **Step 5: Create the entry point (`Program.cs`)**

```csharp
using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

/// <summary>Benchmark host entry point.</summary>
public partial class Program { }
```

- [ ] **Step 6: Add the project to the solution**

```bash
dotnet sln src/IndQuestEnums.sln add src/IndQuestEnums.Benchmarks/IndQuestEnums.Benchmarks.csproj
```

- [ ] **Step 7: Verify it builds in Release**

Run: `dotnet build src/IndQuestEnums.Benchmarks/IndQuestEnums.Benchmarks.csproj -c Release`
Expected: Build succeeded. If restore reports BenchmarkDotNet 0.14.0 is incompatible with net10, bump the central pin: edit `src/Directory.Packages.props` to the newest `BenchmarkDotNet` version (`dotnet package search BenchmarkDotNet --take 1`) and rebuild.

- [ ] **Step 8: Verify benchmarks are discoverable**

Run: `dotnet run -c Release --project src/IndQuestEnums.Benchmarks -- --list flat`
Expected: lists `LookupBenchmarks.*` and `ConverterBenchmarks.*` entries. (Full run is `dotnet run -c Release --project src/IndQuestEnums.Benchmarks -- --filter *` — not required here.)

- [ ] **Step 9: Commit**

```bash
git add src/IndQuestEnums.Benchmarks src/IndQuestEnums.sln src/Directory.Packages.props
git commit -m "test: add BenchmarkDotNet performance project for hot lookup/convert paths"
```

---

### Task 4: Extend mutation testing (JSON converter + EF config)

**Files:**
- Modify: `src/IndQuestEnums.Tests/stryker-config.json` (widen mutate glob to cover the JSON converter)
- Create: `src/IndQuestEnums.Tests/stryker-config.ef.json`

**Interfaces:**
- Consumes: existing Stryker dotnet tool (`.config/dotnet-tools.json`, v4.14.2).
- Produces: two Stryker configs invoked by CI (Task 5).

- [ ] **Step 1: Widen the core config's mutate glob**

In `src/IndQuestEnums.Tests/stryker-config.json`, replace the `mutate` array so it also covers the in-core JSON converter (currently only `EnumModel.cs` is mutated):
```json
    "mutate": [
      "**/EnumModel.cs",
      "**/EnumModelJsonConverter.cs"
    ],
```

- [ ] **Step 2: Create the EF companion Stryker config**

Create `src/IndQuestEnums.Tests/stryker-config.ef.json`:
```json
{
  "$schema": "https://raw.githubusercontent.com/stryker-mutator/stryker-net/master/src/Stryker.Core/Stryker.Core/Schemas/stryker-config.json",
  "stryker-config": {
    "project-info": {
      "name": "IndQuestEnums.EntityFramework",
      "module": "IndQuestEnums.EntityFramework"
    },
    "project": "IndQuestEnums.EntityFramework.csproj",
    "test-projects": [
      "IndQuestEnums.Tests.csproj"
    ],
    "test-runner": "mtp",
    "mutate": [
      "**/EnumModelConverter.cs",
      "**/EnumModelComparer.cs"
    ],
    "coverage-analysis": "perTest",
    "reporters": [
      "html",
      "progress",
      "cleartext"
    ],
    "thresholds": {
      "high": 90,
      "low": 80,
      "break": 0
    },
    "concurrency": 4,
    "mutation-level": "Standard"
  }
}
```

- [ ] **Step 3: Restore the Stryker tool**

Run: `dotnet tool restore`
Expected: `dotnet-stryker` (4.14.2) restored.

- [ ] **Step 4: Run the core config (now incl. JSON converter)**

```bash
cd src/IndQuestEnums.Tests
dotnet stryker -f stryker-config.json
cd ../..
```
Expected: completes, prints a mutation score, exit code 0 (because `break: 0`). Both `EnumModel.cs` and `EnumModelJsonConverter.cs` appear in the report.

- [ ] **Step 5: Run the EF config**

```bash
cd src/IndQuestEnums.Tests
dotnet stryker -f stryker-config.ef.json
cd ../..
```
Expected: completes, prints a score, exit code 0. `EnumModelConverter.cs` / `EnumModelComparer.cs` appear.

- [ ] **Step 6: Commit**

```bash
git add src/IndQuestEnums.Tests/stryker-config.json src/IndQuestEnums.Tests/stryker-config.ef.json
git commit -m "test: mutate JSON converter and add EF companion Stryker config"
```

---

### Task 5: GitHub Actions CI workflow

**Files:**
- Create: `.github/workflows/ci.yml`

**Interfaces:**
- Consumes: solution build/test commands, both Stryker configs (Task 4).
- Produces: PR/push gating CI with a non-blocking mutation report artifact.

- [ ] **Step 1: Create the workflow**

```yaml
name: CI

on:
  push:
    branches: [dev, main]
  pull_request:
    branches: [dev, main]

jobs:
  build-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - name: Restore
        run: dotnet restore src/IndQuestEnums.sln
      - name: Build
        run: dotnet build src/IndQuestEnums.sln -c Release --no-restore
      - name: Test
        run: dotnet test src/IndQuestEnums.sln -c Release --no-build

  mutation:
    runs-on: ubuntu-latest
    needs: build-test
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - name: Restore tools
        run: dotnet tool restore
      - name: Mutate core (EnumModel + JSON converter)
        working-directory: src/IndQuestEnums.Tests
        run: dotnet stryker -f stryker-config.json
      - name: Mutate EF companion
        working-directory: src/IndQuestEnums.Tests
        run: dotnet stryker -f stryker-config.ef.json
      - name: Upload mutation reports
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: stryker-reports
          path: src/IndQuestEnums.Tests/StrykerOutput/**/reports/*.html
          if-no-files-found: warn
```

- [ ] **Step 2: Validate the YAML parses**

Run: `node -e "require('fs').readFileSync('.github/workflows/ci.yml','utf8'); console.log('read ok')"` then visually confirm indentation. (No `act` run required; syntax is validated by GitHub on push.)
Expected: prints `read ok` and the file matches Step 1 exactly.

- [ ] **Step 3: Commit**

```bash
git add .github/workflows/ci.yml
git commit -m "ci: add build, test, and mutation-report workflow"
```

---

### Task 6: Community / policy docs

**Files:**
- Create: `.github/CONTRIBUTING.md`
- Create: `.github/SECURITY.md`
- Create: `.github/CODE_OF_CONDUCT.md`
- Create: `.github/PULL_REQUEST_TEMPLATE.md`
- Create: `.github/ISSUE_TEMPLATE/bug_report.md`
- Create: `.github/ISSUE_TEMPLATE/feature_request.md`
- Create: `CHANGELOG.md`
- Modify: `README.md` (append non-commercial Exxerpro maintainer credit)

**Interfaces:**
- Consumes: ADR location from Task 1, command set from Tasks 2–4.
- Produces: none (terminal docs).

- [ ] **Step 1: Create `.github/CONTRIBUTING.md`**

```markdown
# Contributing to IndQuestEnums

Thanks for helping improve the house SmartEnum base. This repo ships two NuGet
packages: `IndQuestEnums` (dependency-free core) and `IndQuestEnums.EntityFramework`.

## Ground rules

- The **core** project stays dependency-free — never add a `PackageReference` to
  `IndQuestEnums`. EF/third-party concerns go in a companion package (see
  [ADR-0001](../docs/architecture/adr/ADR-0001.md) and
  [ADR-0002](../docs/architecture/adr/ADR-0002.md)).
- TDD: write the failing test first (xUnit v3 + Shouldly). Builds are
  warnings-clean; keep them that way.
- One version for both packages — set it once in `src/Directory.Build.props`.

## Branch model

Work off `dev`; open PRs into `dev`. `main` is the release branch.

## Local commands (PowerShell, from repo root)

```powershell
dotnet build src/IndQuestEnums.sln -c Release
dotnet test src/IndQuestEnums.sln
dotnet tool restore; cd src/IndQuestEnums.Tests; dotnet stryker -f stryker-config.json; cd ../..
dotnet run -c Release --project src/IndQuestEnums.Benchmarks -- --filter *
```

## Pull requests

- Include tests for behavior changes.
- Update `CHANGELOG.md` under `[Unreleased]`.
- Add or update an ADR for design-level decisions.

Maintained by [Exxerpro Solutions](https://www.exxerpro.com).
```

- [ ] **Step 2: Create `.github/SECURITY.md`**

```markdown
# Security Policy

## Supported versions

| Version | Supported |
|---------|-----------|
| 1.1.x   | ✅        |
| < 1.1   | ❌        |

## Reporting a vulnerability

Please report security issues privately to **abel.briones@exxerpro.com**.
Do not open a public issue for security reports. We aim to acknowledge within
5 business days.

Maintained by [Exxerpro Solutions](https://www.exxerpro.com).
```

- [ ] **Step 3: Create `.github/CODE_OF_CONDUCT.md`**

```markdown
# Code of Conduct

This project adopts the [Contributor Covenant](https://www.contributor-covenant.org),
version 2.1. By participating, you are expected to uphold this code.

## Our pledge

We pledge to make participation a harassment-free experience for everyone,
regardless of age, body size, disability, ethnicity, gender identity, level of
experience, nationality, personal appearance, race, religion, or sexual identity.

## Enforcement

Report unacceptable behavior to **abel.briones@exxerpro.com**. All complaints will
be reviewed and investigated promptly and fairly.

Full text: https://www.contributor-covenant.org/version/2/1/code_of_conduct/
```

- [ ] **Step 4: Create `.github/PULL_REQUEST_TEMPLATE.md`**

```markdown
## Summary

<!-- What does this change and why? -->

## Checklist

- [ ] Tests added/updated and passing (`dotnet test src/IndQuestEnums.sln`)
- [ ] Build is warnings-clean in Release
- [ ] `CHANGELOG.md` updated under `[Unreleased]`
- [ ] ADR added/updated if this is a design decision
- [ ] Core project still dependency-free (if touched)
```

- [ ] **Step 5: Create `.github/ISSUE_TEMPLATE/bug_report.md`**

```markdown
---
name: Bug report
about: Report incorrect behavior
title: "[bug] "
labels: bug
---

**What happened**

**Expected behavior**

**Repro (enum definition + call)**

```csharp
// minimal repro
```

**Package + version**

- Package: IndQuestEnums / IndQuestEnums.EntityFramework
- Version:
- .NET SDK:
```

- [ ] **Step 6: Create `.github/ISSUE_TEMPLATE/feature_request.md`**

```markdown
---
name: Feature request
about: Suggest an enhancement
title: "[feat] "
labels: enhancement
---

**Problem / use case**

**Proposed solution**

**Does it keep the core dependency-free?**

<!-- New third-party dependencies belong in a companion package, not the core. -->
```

- [ ] **Step 7: Create `CHANGELOG.md`**

```markdown
# Changelog

All notable changes to this project are documented here. The format is based on
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and this project adheres
to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Repo governance: GitHub Actions CI, community/policy docs, central build config.
- BenchmarkDotNet performance project for the hot lookup/convert paths.
- Stryker mutation coverage for the JSON converter and the EF companion.
- BMAD methodology (`.bmad-core/`).

### Changed
- ADRs moved to `docs/architecture/adr/`.

## [1.1.0] - 2026-06-28

### Added
- `InvalidValue<T>()` public resolver and `InvalidState` constant.
- In-core `EnumModelJsonConverter` (System.Text.Json), per ADR-0002.

## [1.0.1] - 2026-06

### Added
- Mutation testing (Stryker.NET), net10 dependency bumps, AGENTS.md.

## [1.0.0] - 2026-06

### Added
- Initial release: dependency-free `EnumModel` core + `IndQuestEnums.EntityFramework` companion.
```

- [ ] **Step 8: Append the non-commercial Exxerpro credit to `README.md`**

Add at the very end of `README.md` (a credit line, not marketing):
```markdown

---

Maintained by [Exxerpro Solutions](https://www.exxerpro.com) — the house SmartEnum
base shared across IndTrace, ExxerCube.Prisma, and EMIP/CubeXplorer.
```

- [ ] **Step 9: Commit**

```bash
git add .github CHANGELOG.md README.md
git commit -m "docs: add governance/community docs, changelog, and Exxerpro credit"
```

---

### Task 7: Install BMAD methodology

**Files:**
- Create: `.bmad-core/` (generated by the BMAD installer — committed)
- Modify: `.gitignore` (ignore transient node output if the installer leaves any)

**Interfaces:**
- Consumes: `npx` (Node 22 confirmed available).
- Produces: committed `.bmad-core/` so the methodology travels with the repo.

- [ ] **Step 1: Run the BMAD installer (non-interactive attempt first)**

Run: `npx --yes bmad-method install --full --ide claude-code --directory .`
Expected: installs into `.bmad-core/`. If the installer ignores flags and prompts interactively, STOP and ask the user to run `npx bmad-method install` themselves via the `!` prefix (it needs a TTY), choosing this repo's root as the target and `claude-code` as the IDE. Then continue from Step 2.

- [ ] **Step 2: Confirm what was generated**

```bash
ls -la .bmad-core 2>/dev/null; git status --short | head -40
```
Expected: `.bmad-core/` exists with agent/template content.

- [ ] **Step 3: Gitignore transient node output (only if present)**

If the installer created `node_modules/` or a stray lockfile at the repo root, append to `.gitignore`:
```
# Node / BMAD installer transient output
node_modules/
```
Do **not** ignore `.bmad-core/` — it must be committed.

- [ ] **Step 4: Build still green (no .NET impact expected)**

Run: `dotnet build src/IndQuestEnums.sln -c Release`
Expected: Build succeeded (BMAD adds no .NET projects).

- [ ] **Step 5: Commit**

```bash
git add .bmad-core .gitignore
git commit -m "chore: install BMAD methodology (.bmad-core)"
```

---

## Self-Review

**Spec coverage:**
- §1 Repo organization → Task 1. ✅
- §2 Central build config → Task 2. ✅
- §3 GitHub Actions CI → Task 5. ✅
- §4 Community/policy docs → Task 6. ✅
- §5 Mutation testing (extend) → Task 4 (JSON converter glob fix + EF config). ✅
- §6 BenchmarkDotNet → Task 3. ✅
- §7 BMAD → Task 7. ✅
- Exxerpro `https://www.exxerpro.com` mention → Task 2 (metadata) + Task 6 (README/CONTRIBUTING/SECURITY/CoC). ✅
- Security contact `abel.briones@exxerpro.com` → Task 6 (SECURITY.md, CoC). ✅
- Legal name `Exxerpro Solutions SA de CV` → Task 2 (Directory.Build.props). ✅

**Note vs spec:** The spec assumed the existing Stryker config already covered the JSON converter "because it mutates the whole core project." It does not — the config restricts `mutate` to `**/EnumModel.cs`. Task 4 Step 1 corrects this by widening the glob. The spec's "Authors=IndQuest mismatch" is already partially fixed in the live csproj (`Authors` already reads Exxerpro); Task 2 still centralizes it to a single source.

**Placeholder scan:** No TBD/TODO/"handle edge cases"/"similar to Task N" — all code and config shown in full.

**Type consistency:** Benchmark code uses only confirmed public members (`FromValue<T>`, `FromName<T>`, `FromDisplayName<T>`, `Warm<T>`, implicit operators, `EnumModelConverter<T>.ConvertToProvider/ConvertFromProvider`, `EnumModelJsonConverter`). `BenchEnum` follows the AGENTS.md consumer pattern (public parameterless ctor + `Invalid` field).
