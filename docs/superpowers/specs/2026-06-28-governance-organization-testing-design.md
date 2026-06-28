# Design: Governance, Organization & Testing for IndQuestEnums

- **Date:** 2026-06-28
- **Author:** Abel Briones (hebelmx) with Claude
- **Status:** Approved (design) — pending implementation plan
- **Related:** [ADR-0001](../../architecture/adr/ADR-0001.md), [ADR-0002](../../architecture/adr/ADR-0002.md)

## Goal

IndQuestEnums was recently extracted from three consumer projects (IndTrace,
ExxerCube.Prisma, EMIP/CubeXplorer). It is a mature, well-documented two-package
NuGet library, but lacks repo-level governance, a tidy layout, the house BMAD
methodology, and performance testing. This change adds those without regressing
the ADR-0001/0002 design invariants.

Mutation testing (Stryker.NET 4.14.2) already exists and is extended, not added.

## Scope

In scope:
1. Light repo reorganization (ADRs → `docs/architecture/adr/`, new `docs/`).
2. Central build config (`Directory.Build.props`, `Directory.Packages.props`).
3. GitHub Actions CI (build + test + mutation report).
4. Community/policy docs (CONTRIBUTING, SECURITY, CODE_OF_CONDUCT, CHANGELOG, templates).
5. Mutation testing extended to the EF companion + wired into CI.
6. BenchmarkDotNet performance project.
7. BMAD methodology installed and committed (`.bmad-core/`).

Out of scope (explicitly deferred):
- Release/publish (tag-driven NuGet push) workflow — manual pack/push stays.
- Migrating IndTrace/Prisma consumers (tracked separately, ADR-0002 / IndTrace #37).
- Any change to `EnumModel` runtime behavior or public API.

## Design invariants preserved

- Core `IndQuestEnums` stays dependency-free (ADR-0001 #1). No new package
  references on the core project.
- Version stays per-package-correct: core and EF companion publish at the same
  version, now from a single source (see Central build config).
- Warnings-as-errors and the existing xUnit v3 + Shouldly + MTP test stack are
  unchanged. Stryker `break=0` (report-only) convention preserved.

## 1. Repo organization

| Action | From | To |
|---|---|---|
| Move | `ADR-0001.md` | `docs/architecture/adr/ADR-0001.md` |
| Move | `ADR-0002.md` | `docs/architecture/adr/ADR-0002.md` |
| Create | — | `docs/` (governance docs home) |

- Use `git mv` to preserve history.
- Update ADR references in `AGENTS.md` (lines referencing `ADR-0001.md` /
  `ADR-0002.md` and the "Decision record" + "Repository layout" sections) and in
  `README.md`.
- Root keeps: `LICENSE.txt`, `global.json`, `AGENTS.md`, `README.md`, `CHANGELOG.md`.
- Community-health files live under `.github/` (GitHub resolves them there).

## 2. Central build config

**`src/Directory.Build.props`** — imported automatically by all projects under `src/`.
Hoist common properties currently duplicated across the three csproj files:

- `TargetFramework=net10.0`, `Nullable=enable`, `ImplicitUsings=enable`,
  `LangVersion=latest`.
- Shared package metadata: `Authors`, `Company`, `Copyright`, `Product`,
  `<Version>` (single source of truth — resolves the "bump both together" gotcha).
- `GenerateDocumentationFile=true` only on packable projects — guard with a
  property (`IsPackable`) so test/benchmark projects don't require XML docs.
- Fix the `Authors=IndQuest` vs Exxerpro copyright mismatch (AGENTS.md gotcha):
  set `Authors`/`Company` to `Exxerpro Solutions SA de CV` (the legal name),
  `Copyright` to `© 2026 Exxerpro Solutions SA de CV`, and set
  `PackageProjectUrl` / a homepage mention to `https://exxerpro.com`.

Per-csproj files keep only what is genuinely package-specific (`PackageId`,
`Description`, `PackageTags`, project references).

**`src/Directory.Packages.props`** — enable Central Package Management
(`ManagePackageVersionsCentrally=true`). Move all `PackageVersion` pins here:
`Microsoft.EntityFrameworkCore`, `xunit.v3`, `Shouldly`, test SDK, coverage,
`BenchmarkDotNet`. Strip `Version=` attributes from the csproj `PackageReference`s.

Acceptance: `dotnet build src/IndQuestEnums.sln -c Release` and
`dotnet test src/IndQuestEnums.sln` both pass unchanged after the refactor;
`dotnet pack` produces the same package version for both packages.

## 3. GitHub Actions CI

**`.github/workflows/ci.yml`**

- Triggers: `pull_request` and `push` to `dev` and `main`.
- Runner: `ubuntu-latest`.
- Jobs:
  - **build-test:** `actions/setup-dotnet` (10.0.x) → `dotnet restore` →
    `dotnet build -c Release` (warnings-as-errors enforced) → `dotnet test`.
  - **mutation:** `dotnet tool restore` → run Stryker for the core config (and
    the new EF config) → upload the `StrykerOutput/` HTML report as an artifact.
    `break=0` means this job reports but does not fail the build.

Acceptance: workflow validates (YAML) and the documented commands match the
PowerShell commands in AGENTS.md.

## 4. Community / policy docs

- `.github/CONTRIBUTING.md` — build/test/pack/mutation commands, branch model
  (`dev` → `main`), warnings-as-errors + TDD expectations, ADR pointer.
- `.github/SECURITY.md` — supported versions + private disclosure contact
  (`abel.briones@exxerpro.com`).
- `.github/CODE_OF_CONDUCT.md` — Contributor Covenant 2.1.
- `CHANGELOG.md` (root) — Keep a Changelog format, seeded with the 1.0.0 → 1.0.1
  → 1.1.0 history visible in git + ADR-0002.
- `.github/PULL_REQUEST_TEMPLATE.md` — checklist (tests, docs, ADR, changelog).
- `.github/ISSUE_TEMPLATE/bug_report.md` + `feature_request.md`.
- **Exxerpro mention** (non-commercial): a short maintainer/credits line linking
  [Exxerpro Solutions](https://exxerpro.com) in the `README.md` footer and
  `CONTRIBUTING.md`, plus `PackageProjectUrl=https://exxerpro.com` in package
  metadata. Just a mention of who maintains it — not a marketing blurb.

## 5. Mutation testing (extend)

- Existing `src/IndQuestEnums.Tests/stryker-config.json` targets the core
  project (`mtp` runner mandatory) — unchanged; already covers the new
  `EnumModelJsonConverter.cs` since it mutates the whole core project.
- Add `src/IndQuestEnums.Tests/stryker-config.ef.json` with
  `"project": "IndQuestEnums.EntityFramework.csproj"` to mutate the EF companion.
- Both invoked from the CI mutation job. `break=0` retained.

Acceptance: `dotnet stryker -f stryker-config.json` and
`-f stryker-config.ef.json` each run to completion and emit a score.

## 6. Performance testing — BenchmarkDotNet

**`src/IndQuestEnums.Benchmarks/`** — console project, `net10.0`,
`IsPackable=false`, `OutputType=Exe`, Release-only meaningful. Added to the
solution. References core + EF packages (project references).

Benchmark classes (hot paths from ADR-0002, ~1,600 conversions/report):
- `FromValueBenchmarks` — warm cache hit vs first-call build.
- `FromNameBenchmarks` / display-name — linear scan cost.
- `ConversionBenchmarks` — implicit `→int` and `→string`.
- `EfConverterBenchmarks` — `EnumModelConverter` int⇄enum round-trip.
- `JsonConverterBenchmarks` — `EnumModelJsonConverter` serialize/deserialize.

Uses a representative test enum (reuse or mirror `TestEnums`). Run via
`dotnet run -c Release --project src/IndQuestEnums.Benchmarks`. Not run in CI
(deferred; no perf gate this round).

Acceptance: project builds in Release and `--list flat` enumerates the benchmarks.

## 7. BMAD methodology

- Run `npx bmad-method install`, target this repo, IDE = Claude Code.
- Commit the generated `.bmad-core/` (agents + templates) so it travels with the repo.
- Ensure BMAD-generated node artifacts that should NOT be committed
  (`node_modules/`, lockfiles if undesired) are gitignored; `.bmad-core/` is kept.
- **Risk:** the installer is interactive. Attempt non-interactive flags first; if
  it blocks, the user runs `npx bmad-method install` via `!` and the rest is wired
  up around the committed output.

Acceptance: `.bmad-core/` exists and is committed; `.gitignore` updated for any
transient node output.

## Sequencing / risk

Order chosen so each step builds green before the next:
1. Reorg (ADR move + reference updates) — verify build/test still green.
2. Central build config — verify build/test/pack unchanged (highest regression risk).
3. Benchmarks project — additive.
4. Mutation EF config — additive.
5. CI workflow — additive.
6. Community docs + CHANGELOG — additive.
7. BMAD install — additive, may need user interaction.

Each step is independently committable. Step 2 is the only one that can break the
build; it is validated against a full build + test + pack before proceeding.

## Testing strategy

- After steps 1–2: full `dotnet build -c Release` + `dotnet test` must pass with
  no new warnings; `dotnet pack` must emit both packages at the unified version.
- After step 3: benchmarks build + list.
- After step 4: both Stryker configs run.
- After step 5: YAML lint / `act`-free static validation of the workflow.
- No production source (`EnumModel.cs`, converters) is modified; existing tests
  remain the behavioral guard.
