# Changelog

All notable changes to Subscrio for ABP are recorded here.

## [Unreleased]

## [0.5.0] - 2026-09-23

### Changed

- Use published Subscrio.Core 0.5.1 by default; local core builds remain available through UseLocalSubscrioCore.
- The sample verifies add-on attachment, detachment, and timed overrides through ABP IFeatureChecker.
- Allow a SQL Server instance to be configured for the sample while retaining LocalDB as the default.

## [0.4.0] - 2026-09-20

### Added

- The first public `Subscrio.Abp` package for .NET 8, .NET 9, and .NET 10.
- Tenant-based and user-based customer key resolvers for ABP applications.
- A Subscrio feature value provider that participates in ABP's normal feature resolution pipeline.
- Catalog conversion from ABP feature definitions to Subscrio configuration.
- A runnable SQL Server LocalDB sample and integration tests.
