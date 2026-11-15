# Changelog

All notable changes to this project are documented here. The format is based on
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and this project adheres
to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2026-11-15

### Added
- `.NET 11.0` (`net11.0`) target framework support alongside `.NET 10.0` (`net10.0` LTS) across `IndQuestEnums`, `IndQuestEnums.EntityFramework`, tests, and benchmarks (`TargetFrameworks: net10.0;net11.0`).
- Pinned `.NET SDK 11.0.100` (`rollForward: latestFeature`) in `global.json` and updated GitHub Actions CI (`ci.yml`) and Release (`release.yml`) workflows to provision both `10.0.x` and `11.0.x`.
- Documented **ADR-0003** (`docs/architecture/adr/ADR-0003.md`) and BMAD Party Mode keepsake (`_bmad-output/party-mode/2026-11-15-csharp15-unions-evaluation.html`): evaluated C# 15 / .NET 11 `union` types and `closed` hierarchies and opted out to preserve cross-assembly `EnumModel` inheritance, singleton value-identity semantics, `where TEnum : new()` factory/converter constraints, and zero-allocation JSON/EF `int` conversion.
- Repo governance: GitHub Actions CI, community/policy docs, central build config.
- BenchmarkDotNet performance project for the hot lookup/convert paths.
- Stryker mutation coverage for the JSON converter and the EF companion.
- BMAD methodology upgraded to v6.12.4 (`bmb` v2.2.2; agent skills in `.claude/skills/`, module config in `_bmad/`).

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
