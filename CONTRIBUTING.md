# Contributing

Thanks for contributing to `Subscrio.Abp`.

## Prerequisites

- .NET 10 SDK
- SQL Server Express LocalDB to run the console sample

The reusable package targets .NET 8, .NET 9, and .NET 10. The sample and tests run on .NET 10.

## Build and test

From the repository root:

```powershell
dotnet restore
dotnet build .\Subscrio.Abp.Sample.slnx
dotnet test .\Subscrio.Abp.Sample.slnx --no-build
```

Run the LocalDB sample:

```powershell
dotnet run --project .\src\Subscrio.Abp.Sample
```

### Developing against local Core source

Normal builds use the released `Subscrio.Core` NuGet package, even when a local Core checkout exists. The sample references the ABP module project in this repository.

To work on both repositories together, explicitly enable the local Core project at `../../core/dotnet/src/Subscrio.Core.csproj`, relative to this repository root:

```powershell
dotnet build .\Subscrio.Abp.Sample.slnx -p:UseLocalSubscrioCore=true
dotnet run --project .\src\Subscrio.Abp.Sample -p:UseLocalSubscrioCore=true
```

The local project must exist when this option is enabled. Before submitting changes, rerun the normal build and test commands above against NuGet. Allow restore when switching dependency modes; do not use `--no-restore` or reuse a previous build with `--no-build` until you have rebuilt in the selected mode.

## Pull requests

- Keep changes focused and include tests for behavior changes.
- Keep the package version in `Directory.Build.props` aligned with `Subscrio.Core`.
- Do not commit database files, packages, credentials, local NuGet configuration, or development settings.
- Confirm that the solution builds without warnings before opening a pull request.

## Code of conduct

See [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).
