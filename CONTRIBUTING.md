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

## Pull requests

- Keep changes focused and include tests for behavior changes.
- Keep the package version in `Directory.Build.props` aligned with `Subscrio.Core`.
- Do not commit database files, packages, credentials, local NuGet configuration, or development settings.
- Confirm that the solution builds without warnings before opening a pull request.

## Code of conduct

See [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).
