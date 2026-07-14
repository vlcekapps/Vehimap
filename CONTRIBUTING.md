# Contributing To Vehimap

Thank you for helping improve Vehimap. Contributions written manually, with an IDE, or with automation are equally welcome. No AI tooling is required to build, test, review, or contribute to this repository.

## Start Here

1. Read the [development environment guide](dotnet/docs/DEVELOPMENT.md).
2. Fork or clone the repository and create a focused branch.
3. Build and test the solution before making changes.
4. Keep accessibility, localization, storage compatibility, and user data safety in scope for every user-facing change.
5. Open a pull request that explains the behavior change and how it was tested.

## Project Rules

- Vehimap is licensed under `GPL-3.0-or-later`. Contributions are accepted under the same license.
- Commit messages are written in English.
- New user-interface text must be added to both English and Czech `.resx` resources. Do not translate user-entered data.
- New interactive UI must be keyboard accessible and expose stable automation metadata. Follow [ACCESSIBILITY.md](dotnet/docs/ACCESSIBILITY.md).
- Data changes must preserve SQLite safety, backup/restore behavior, and supported 1.x migration/import paths.
- Do not commit generated artifacts from `dotnet/artifacts`, `bin`, or `obj`.

## Required Checks

From the `dotnet` directory, run:

```text
dotnet restore Vehimap.sln
dotnet build Vehimap.sln --configuration Release
dotnet test Vehimap.sln --configuration Release
```

Platform-specific UI, packaging, storage, localization, and accessibility checks are documented in the [development environment guide](dotnet/docs/DEVELOPMENT.md) and the [developer README](dotnet/README.md).

Android contributors use the separate `dotnet/Vehimap.Android.sln` so desktop contributors do not need a mobile workload. Run `pwsh ./dotnet/build/Test-DotnetDeveloperEnvironment.ps1 -IncludeAndroidTools` and the Android readiness command documented in the development guide before submitting Android changes.
