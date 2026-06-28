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
