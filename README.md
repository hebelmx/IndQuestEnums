# IndQuestEnums

Owned NuGet package providing the house **SmartEnum base** (`EnumModel`) — the single source
of truth for strongly-typed enumerations across IndTrace, ExxerCube.Prisma, EMIP/CubeXplorer,
and future solutions. Named for consistency with its sibling `IndQuestResults`.

> **Seed folder.** This directory was scaffolded as a starting point. Build the real package /
> solution from here in a fresh session, then publish to nuget.org (or the house private feed).
> The decision and rationale are recorded in `ADR-0001` (copied here as `ADR-0001.md`; the
> authoritative copy lives in the EMIP repo at `docs/architecture/adr/`).

## Why this package exists

Two divergent in-house copies already exist and are drifting:

- **IndTrace** `Core/Domain/Enum/Enumeration.cs` — the original. Linear `FromValue`, couples to
  `LookUpTable`, and **carries a display-name bug** (assigns the unset `DisplayName` property
  instead of the constructor parameter, so display names silently collapse to `Name`).
- **ExxerCube.Prisma** `01 Core/Domain/Enum/EnumModel.cs` — "production-tested, ported from
  IndTrace." Bug **fixed**; adds an O(1) thread-safe `ConcurrentDictionary` lookup cache; couples
  to `IEnumModel`/`ILookupEntity`/`EnumLookUpTable` + an EF `EnumModelConversions` helper.

A third copy would keep diverging. This package consolidates the **bug-fixed, O(1)-cached** core.

## Design decisions (from ADR-0001)

1. **Not bundled into `IndQuestResults`** — different concern; bundling would couple semver and
   force every `Result<T>` consumer to take a SmartEnum base.
2. **Core is dependency-free** — no EF, no lookup-table machinery — so it is safe in a pure Domain.
   - `src/EnumModel.cs` is that core (provided here as the starting point; namespace `IndQuestEnums`).
3. **EF integration is a separate companion package** `IndQuestEnums.EntityFramework` (a generic
   `ValueConverter<TEnum,int>` + `ValueComparer<TEnum>`), referenced only by Infrastructure.
4. **Based on the Prisma (bug-fixed + cached) version**, not the IndTrace original.

## Suggested layout to grow into

```
IndQuestEnums/
├── README.md
├── ADR-0001.md
├── src/
│   └── EnumModel.cs                         # ← provided (core; dependency-free)
├── IndQuestEnums/                           # core csproj (net10.0, no deps)
│   └── IndQuestEnums.csproj
├── IndQuestEnums.EntityFramework/           # EF companion (generic converter + comparer)
│   └── IndQuestEnums.EntityFramework.csproj
├── IndQuestEnums.Tests/                     # xUnit v3 + Shouldly
│   └── EnumModelTests.cs                    # FromValue/FromName/Invalid/equality/cache round-trips
└── IndQuestEnums.sln
```

## Example derived SmartEnum (consumer pattern)

```csharp
using IndQuestEnums;

public sealed class ZoneTag : EnumModel
{
    public static readonly ZoneTag Invalid = new(EnumModel.InvalidState, EnumModel.InvalidName);
    public static readonly ZoneTag Z1 = new(1, "Z1", "Zone 1 — production/reasoning");
    public static readonly ZoneTag Z2 = new(2, "Z2", "Zone 2 — grader/validation");
    public static readonly ZoneTag Z3 = new(3, "Z3", "Zone 3 — held-out orchestrator");

    public ZoneTag() { }                                  // required for new() factory fallback
    private ZoneTag(int value, string name, string displayName = "")
        : base(value, name, displayName) { }

    public static ZoneTag FromValue(int value) => FromValue<ZoneTag>(value);
    public static ZoneTag FromName(string name) => FromName<ZoneTag>(name);
}
```

## EF companion sketch (for `IndQuestEnums.EntityFramework`)

```csharp
// Generic value converter: store the int, rehydrate via the cached FromValue.
public sealed class EnumModelConverter<TEnum> : ValueConverter<TEnum, int>
    where TEnum : EnumModel, new()
{
    public EnumModelConverter()
        : base(e => e.Value, v => EnumModel.FromValue<TEnum>(v)) { }
}

public sealed class EnumModelComparer<TEnum> : ValueComparer<TEnum>
    where TEnum : EnumModel
{
    public EnumModelComparer()
        : base((a, b) => a!.Value == b!.Value, e => e.Value, e => e) { }
}
```

## Next steps (you take it from here)

1. `dotnet new classlib` for the core (`net10.0`, `LangVersion=latest`, `Nullable=enable`,
   `GenerateDocumentationFile=true`); drop in `src/EnumModel.cs`.
2. Add the EF companion + a test project (xUnit v3 mtp-v2 + Shouldly), prove: value/name/display
   round-trips, `Invalid` fallback, equality by type+value, cache correctness, duplicate-value safety.
3. Pack + publish `IndQuestEnums` (and `IndQuestEnums.EntityFramework`) to nuget.org.
4. Back in EMIP: pin `IndQuestEnums` in `Src/Directory.Packages.props`, reference it from the Domain
   csproj, add `global using IndQuestEnums;`, and finish Story 1.2 Task 5 (the SmartEnums).
