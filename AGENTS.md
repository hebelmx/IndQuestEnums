# AGENTS.md — IndQuestEnums

Context for AI agents working in this repository. Read this first; it captures the
"why" that the source and git history don't make obvious.

- **Authors / owners:** Abel Briones / Exxerpro Solutions SA de CV
- **Repo root:** `E:\Dynamic\IndFusion\IndQuestEnums`
- **License:** MIT (`LICENSE.txt`) — © 2026 Exxerpro Solutions SA de CV
- **Decision record:** `ADR-0001.md` (authoritative copy lives in the EMIP repo at
  `docs/architecture/adr/`)

## What this is

An **owned NuGet package** providing the house **SmartEnum base** (`EnumModel`) — the single
source of truth for strongly-typed enumerations across IndTrace, ExxerCube.Prisma,
EMIP/CubeXplorer, and future solutions. Named for consistency with its sibling
`IndQuestResults`.

It ships as **two packages**:

| Package | Purpose | Dependencies |
|---|---|---|
| `IndQuestEnums` | Dependency-free core (`EnumModel`). Safe to reference from a **pure Domain** layer. | none |
| `IndQuestEnums.EntityFramework` | EF Core companion: generic `EnumModelConverter<TEnum>` + `EnumModelComparer<TEnum>`. Consumed only by **Infrastructure**. | `Microsoft.EntityFrameworkCore`, core package |

## Why it exists (the drift problem)

Two divergent in-house copies already exist and keep drifting:

- **IndTrace** `Core/Domain/Enum/Enumeration.cs` — the original. Linear `FromValue`, couples to
  `LookUpTable`, and **carries a display-name bug** (assigns the unset `DisplayName` property
  instead of the constructor parameter, so display names silently collapse to `Name`).
- **ExxerCube.Prisma** `01 Core/Domain/Enum/EnumModel.cs` — "production-tested, ported from
  IndTrace." Bug **fixed**; adds an O(1) thread-safe `ConcurrentDictionary` cache; couples to
  `IEnumModel`/`ILookupEntity`/`EnumLookUpTable` + an EF helper.

A third copy would keep diverging. This package consolidates the **bug-fixed, O(1)-cached** core,
**based on the Prisma version** (carrying the display-name fix forward), with the
lookup-table/`ILookUpTable` machinery **dropped** from the core.

## Design invariants — do not regress these

These are load-bearing decisions from ADR-0001. Preserve them in any change:

1. **Core stays dependency-free.** No EF, no lookup-table machinery in `IndQuestEnums`. Anything
   that needs a third-party dependency belongs in a companion package. This is what keeps it
   safe in a pure Domain.
2. **Not bundled into `IndQuestResults`.** Distinct concern; bundling would couple semver and
   force every `Result<T>` consumer to take a SmartEnum base.
3. **Lookups never throw.** An unmatched value/name resolves to the derived type's `Invalid`
   instance (a `public static readonly` field named `Invalid`), or a default `new TEnum()` if
   none is declared.
4. **Carry the display-name fix forward.** `displayName` falls back to `name` when null/whitespace
   — never silently collapse.

## How `EnumModel` works (the core)

`src/IndQuestEnums/EnumModel.cs` — `abstract class EnumModel : IComparable, IEquatable<EnumModel>`.

- **Instances** are declared as `public static readonly` fields on a derived `sealed` type, each
  with a stable `int Value`, invariant `string Name`, and optional `string DisplayName`.
- **Lookup:** `FromValue<TEnum>` uses a per-type `ConcurrentDictionary` cache, built once via
  reflection (O(1) after warm-up). `FromName` / `FromDisplayName` are linear scans.
- **Duplicate values:** the lookup is built with indexer assignment (not `ToDictionary`), so
  duplicate values **don't throw — last declared wins**.
- **Equality:** by `(type, value)`. Because instances are cached, `==` also behaves as reference
  equality for resolved instances.
- **Conversions:** implicit `EnumModel -> int` (Value) and `EnumModel -> string` (DisplayName);
  `ToString()` returns `DisplayName`; `Deconstruct` yields `(value, name, displayName)`.
- **Sentinels:** `InvalidValue = -1`, `InvalidName = "Invalid Value"`.
- A **parameterless `protected` ctor** sets the invalid state and satisfies the `new()` generic
  constraint the factories require.

### Consumer pattern (how derived enums are written)

```csharp
using IndQuestEnums;

public sealed class ZoneTag : EnumModel
{
    public static readonly ZoneTag Invalid = new(EnumModel.InvalidValue, EnumModel.InvalidName);
    public static readonly ZoneTag Z1 = new(1, "Z1", "Zone 1 — production/reasoning");
    public static readonly ZoneTag Z2 = new(2, "Z2", "Zone 2 — grader/validation");

    public ZoneTag() { }                                  // required for new() factory fallback
    private ZoneTag(int value, string name, string displayName = "")
        : base(value, name, displayName) { }

    public static ZoneTag FromValue(int value) => FromValue<ZoneTag>(value);
    public static ZoneTag FromName(string name) => FromName<ZoneTag>(name);
}
```

Two non-negotiable details for a derived type: a public parameterless ctor (factory fallback)
and a `public static readonly Invalid` field (graceful unmatched resolution).

## EF companion

- `EnumModelConverter<TEnum> : ValueConverter<TEnum,int>` — stores `Value`, rehydrates via the
  cached `FromValue<TEnum>`. Requires `TEnum : EnumModel, new()`.
- `EnumModelComparer<TEnum> : ValueComparer<TEnum>` — compares/hashes by `Value`.

## Repository layout

```
IndQuestEnums/
├── AGENTS.md                  # this file
├── README.md                  # human-facing overview
├── ADR-0001.md                # the decision record
├── LICENSE.txt                # MIT
├── global.json
└── src/
    ├── IndQuestEnums.sln
    ├── Icon.png               # 128×128 NuGet icon (packed)  — Icon.original.png is the source asset
    ├── nupkg/                 # pack output (*.nupkg / *.snupkg)
    ├── IndQuestEnums/                 # core csproj (net10.0, no deps)
    │   └── EnumModel.cs
    ├── IndQuestEnums.EntityFramework/ # EF companion
    │   ├── EnumModelConverter.cs
    │   └── EnumModelComparer.cs
    └── IndQuestEnums.Tests/           # xUnit v3 + Shouldly
        ├── EnumModelTests.cs
        └── TestEnums.cs
```

## Conventions

- **Target framework:** `net10.0`. `Nullable=enable`, `ImplicitUsings=enable`,
  `LangVersion=latest`, `GenerateDocumentationFile=true` on packable projects.
- **Tests:** xUnit **v3** + **Shouldly** (not FluentAssertions). Arrange/Act/Assert layout.
  Test project is `IsPackable=false`.
- **Docs:** public API carries XML doc comments; the core file carries the Exxerpro copyright
  header. Keep both when editing.
- **Packaging metadata** lives in each csproj: `PackageId`, `Version`, `Authors`, `Description`,
  `PackageTags`, `PackageReadmeFile`, `PackageLicenseFile`, `PackageIcon`, symbols (`snupkg`).

## Common commands (PowerShell, from repo root)

```powershell
# Build
dotnet build .\src\IndQuestEnums.sln -c Release

# Test (xUnit v3)
dotnet test .\src\IndQuestEnums.sln

# Pack both packages to src\nupkg (pack each project; EF pack alone emits only its own nupkg)
dotnet pack .\src\IndQuestEnums\IndQuestEnums.csproj -c Release -o .\src\nupkg
dotnet pack .\src\IndQuestEnums.EntityFramework\IndQuestEnums.EntityFramework.csproj -c Release -o .\src\nupkg
```

## Mutation testing (Stryker.NET)

Stryker is pinned as a local dotnet tool in `.config/dotnet-tools.json` (v4.14.2). Config lives
in `src/IndQuestEnums.Tests/stryker-config.json` and targets the **core** project (`EnumModel.cs`).

```powershell
dotnet tool restore                       # once, restores dotnet-stryker
cd .\src\IndQuestEnums.Tests
dotnet stryker                            # reads stryker-config.json
```

**Critical:** `"test-runner": "mtp"` in the config is mandatory. The tests run on xUnit v3, which
uses **Microsoft.Testing.Platform (MTP)**, not classic VSTest. Without `mtp`, Stryker launches the
host but cannot read test failures back — every mutant reports as *Survived* (0 killed) even though
the suite is green. This mirrors the working config in ExxerCube.Prisma. Current score ≈ **83%**;
`break` is `0` (report-only, never fails the build), matching the Prisma convention.

To also mutation-test the EF companion, add a second config with `"project":
"IndQuestEnums.EntityFramework.csproj"` and run it separately (Stryker mutates one source project
per run).

When packing, both `.nupkg`s should contain `Icon.png`, `LICENSE.txt`, and `README.md`. The
NuGet icon must stay ≤ 1 MB (PNG or JPEG); the packed `Icon.png` is 128×128.

## Status & next steps (from README / ADR-0001)

ADR-0001 is **Proposed**; promote to **Accepted** once published and referenced. Remaining work:

1. Publish `IndQuestEnums` (+ `IndQuestEnums.EntityFramework`) to nuget.org or the house private
   feed. (EMIP's `nuget.config` is nuget.org-only + `clear`, so the package must reach a
   reachable feed.)
2. Back in EMIP: pin `IndQuestEnums` in `Src/Directory.Packages.props` (next to `IndQuestResults`),
   reference it from the Domain csproj, add `global using IndQuestEnums;`, and finish Story 1.2
   Task 5 (the `Confidence` / `FaultClass` / `ControlStrategy` / `ZoneTag` SmartEnums).
3. Migrate IndTrace and Prisma to the package opportunistically; retire their local copies.

## Gotchas

- Don't add dependencies to the **core** project — that's the whole point of the split.
- Packing only the EF project does **not** also pack the core; pack each project explicitly.
- The package version is set per-csproj (`<Version>`); bump both together when releasing.
- `Authors` is currently `IndQuest` in the csproj while the license/copyright reads
  Exxerpro Solutions — confirm intended author metadata before publishing.
```
