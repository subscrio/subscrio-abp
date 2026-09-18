# ABP + Subscrio integration sample

This repository is the working implementation for the accompanying ABP and Subscrio article. The article's code should come from this repository so readers can copy the same code that was compiled and run here.

Learn more about Subscrio at [subscrio.com](https://subscrio.com). For the core libraries, project-wide documentation, and cross-cutting issues, visit the main [subscrio/subscrio repository](https://github.com/subscrio/subscrio).

The solution has three projects:

- `Subscrio.Abp` is a small reusable ABP module. It registers the Subscrio feature value provider, supports tenant-based and user-based customer identity, and converts ABP feature definitions into Subscrio feature configuration.
- `Subscrio.Abp.Sample` is the Acme console app from the article. It defines the product, Free and Pro plans, customer, subscription, and customer override.
- `Subscrio.Abp.Tests` verifies tenant-based and user-based customer identity resolution.

The console host keeps the example focused. ABP supports console applications through `AbpApplicationFactory`, so the sample does not need a web server, controller, UI, or authentication setup.

## What the sample proves

- ABP feature definitions are the source of truth for feature names, types, and defaults.
- The same feature keys are used by ABP and Subscrio. There is no feature mapping table in the Acme sample.
- The app reads ABP's definitions and syncs the Subscrio catalog during ABP application initialization.
- A stable ABP tenant ID maps to `tenant-{guid}` in Subscrio.
- Applications without tenants can map the current ABP user to `user-{guid}` instead.
- Acme Manufacturing has a Pro subscription with a customer-specific project limit of 250.
- Application code receives the effective value through ABP's `IFeatureChecker`.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server Express LocalDB](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb)

LocalDB is installed with many Visual Studio configurations. Confirm that the default instance exists:

```powershell
sqllocaldb info MSSQLLocalDB
```

No Docker services are required. The app creates these ignored database files on its first run:

```text
data\subscrio-abp.mdf
data\subscrio-abp_log.ldf
```

## Run the sample

Open PowerShell in this repository:

```powershell
dotnet restore
dotnet run --project .\src\Subscrio.Abp.Sample
```

The console output shows each layer separately:

```text
1. FEATURE DEFAULTS
────────────────────────────
Reports:       false
MaxProjects:   3

2. PRO PLAN VALUES
────────────────────────────
Reports:       true
MaxProjects:   100

3. CUSTOMER VALUES
────────────────────────────
Customer:      Acme Manufacturing
Plan:          Pro

Reports:       true
MaxProjects:   250   (customer override)

Resolved through ABP IFeatureChecker:
Reports:       true
MaxProjects:   250

PASS: defaults → plan → customer override resolved correctly.
```

Run the app again to confirm that catalog sync and demo seeding are idempotent:

```powershell
dotnet run --project .\src\Subscrio.Abp.Sample
```

The second run should report zero created and zero updated catalog records.

Run the integration tests to verify both customer identity modes:

```powershell
dotnet test .\Subscrio.Abp.Sample.slnx
```

The tests cover tenant IDs, missing tenant context, authenticated user IDs, anonymous user context, and invalid resolver registration.

## Add the reusable module to an ABP app

The reusable project targets .NET 8, .NET 9, and .NET 10.

After the package is published, install it with:

```powershell
dotnet add package Subscrio.Abp
```

The package version always matches its `Subscrio.Core` dependency version. Both repositories store that value in `Directory.Build.props`, and the private Subscrio release script updates and packs them together. A complete local Subscrio workspace uses the sibling Core project automatically; a standalone clone restores the matching `Subscrio.Core` NuGet package.

Register Subscrio and configure the product and managed features:

```csharp
[DependsOn(
    typeof(AbpAutofacModule),
    typeof(SubscrioAbpTenantModule)
)]
public sealed class AcmeModule : AbpModule
{
    public override void ConfigureServices(
        ServiceConfigurationContext context)
    {
        context.Services.AddSubscrio(
            new SubscrioConfig
            {
                Database = new DatabaseConfig
                {
                    ConnectionString =
                        context.Services
                            .GetConfiguration()
                            .GetConnectionString("Default")!
                }
            },
            ServiceLifetime.Scoped);

        Configure<SubscrioAbpOptions>(options =>
        {
            options.ProductKey = AppProducts.Acme;
            options.FeatureGroupName = "Acme";
            options.IsManagedFeature = feature =>
                AppFeatures.All.Contains(
                    feature.Name,
                    StringComparer.Ordinal);
        });

        Configure<SubscrioAbpTenantOptions>(options =>
        {
            options.CustomerKeyFactory =
                tenantId => $"tenant-{tenantId:N}";
        });
    }
}
```

### Tenant applications

Depend on `SubscrioAbpTenantModule`. Its resolver reads `ICurrentTenant.Id`. A host context with no tenant returns `null`, which allows ABP's next feature value provider to run.

```csharp
[DependsOn(typeof(SubscrioAbpTenantModule))]
public sealed class AcmeModule : AbpModule
{
    public override void ConfigureServices(
        ServiceConfigurationContext context)
    {
        Configure<SubscrioAbpTenantOptions>(options =>
        {
            options.CustomerKeyFactory =
                tenantId => $"tenant-{tenantId:N}";
        });
    }
}
```

### Applications without tenants

Depend on `SubscrioAbpUserModule`. Its resolver reads `ICurrentUser.Id` and uses the current authenticated user as the Subscrio customer. An anonymous request returns `null`.

```csharp
[DependsOn(typeof(SubscrioAbpUserModule))]
public sealed class AcmeModule : AbpModule
{
    public override void ConfigureServices(
        ServiceConfigurationContext context)
    {
        Configure<SubscrioAbpUserOptions>(options =>
        {
            options.CustomerKeyFactory =
                userId => $"user-{userId:N}";
        });
    }
}
```

Choose one identity module for an application. If customer identity comes from something other than an ABP tenant or user, implement `ISubscrioCustomerKeyResolver` and register that implementation with the base `SubscrioAbpModule`.

`SubscrioAbpOptions` provides `FeatureKeyMapper` and `FeatureDisplayNameFactory` hooks for applications whose naming rules differ from this sample. The Acme app uses the identity mapper, so `acme-max-projects` is the key in both systems.

The module intentionally does not define products or plans. Those are business-specific. The sample's `AcmeCatalogSynchronizer` uses the reusable `AbpSubscrioFeatureCatalog` service to get feature configuration from ABP, then adds the Free and Pro plans before calling Subscrio's configuration sync.

## The application code

The application asks ABP for feature values in the normal way:

```csharp
using (_currentTenant.Change(
    DemoCatalog.TenantId,
    "Acme Manufacturing"))
{
    var reports = await _featureChecker.IsEnabledAsync(
        AppFeatures.Reports);

    var maxProjects = await _featureChecker.GetAsync<int>(
        AppFeatures.MaxProjects);
}
```

The reusable `SubscrioFeatureValueProvider` handles the rest. It asks the selected customer resolver for a tenant or user customer key, then asks Subscrio to resolve the value. Subscrio checks the subscription override, then the plan value, then the feature default.

## Source map for the article

- `src/Subscrio.Abp.Sample/AppFeatures.cs` contains the feature, plan, product, and tenant constants used in the article.
- `src/Subscrio.Abp/SubscrioAbpModule.cs` registers the reusable ABP integration.
- `src/Subscrio.Abp/SubscrioAbpOptions.cs` contains the small set of application-specific hooks.
- `src/Subscrio.Abp/SubscrioAbpTenantModule.cs` selects tenant-based customer identity.
- `src/Subscrio.Abp/SubscrioAbpUserModule.cs` selects user-based customer identity.
- `src/Subscrio.Abp/ISubscrioCustomerKeyResolver.cs` is the extension point for other identity models.
- `tests/Subscrio.Abp.Tests/CustomerKeyResolverTests.cs` verifies both built-in customer resolvers.
- `src/Subscrio.Abp/SubscrioFeatureValueProvider.cs` connects `IFeatureChecker` to Subscrio.
- `src/Subscrio.Abp/AbpSubscrioFeatureCatalog.cs` converts ABP definitions into Subscrio feature configuration.
- `src/Subscrio.Abp.Sample/AcmeCatalogSynchronizer.cs` adds Acme's Free and Pro plans and runs the catalog sync.
- `src/Subscrio.Abp.Sample/DemoDataSeeder.cs` creates Acme Manufacturing, its Pro subscription, and the 250-project override.
- `src/Subscrio.Abp.Sample/DemoRunner.cs` performs the end-to-end checks and prints the three-stage output.

## Stripe integration

The console app does not host ABP's commercial Payment Module or an HTTP webhook endpoint. The Stripe section of the article is an optional web-host extension. Its event handler should use the same customer and plan keys shown here.

The included Stripe visual follows the article's intended path:

```text
Stripe webhook
    → ABP Payment Module
    → ABP distributed subscription event
    → Subscrio event handler
    → updated customer subscription
```

Useful references:

- [ABP features and custom feature value providers](https://abp.io/docs/latest/framework/infrastructure/features)
- [ABP Autofac integration for console applications](https://abp.io/docs/latest/Autofac-Integration)
- [ABP multi-tenancy](https://abp.io/docs/latest/framework/architecture/multi-tenancy)
- [ABP Payment Module](https://abp.io/docs/latest/modules/payment)
- [Subscrio feature resolution](https://docs.subscrio.com/reference/feature-checker/)
- [Subscrio configuration sync](https://docs.subscrio.com/reference/config-sync/)

## Blog visuals

The `docs/visuals` folder contains SVG files for the website and Mermaid source files for later edits:

- ABP and Subscrio architecture flow
- Customer entitlement resolution
- Stripe to ABP to Subscrio event flow

## Contributing and security

- [Contributing guide](CONTRIBUTING.md)
- [Security policy](SECURITY.md)
- [Code of conduct](CODE_OF_CONDUCT.md)

This repository is licensed under the [MIT License](LICENSE).
