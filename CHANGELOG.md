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
