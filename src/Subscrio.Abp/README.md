# Subscrio.Abp

`Subscrio.Abp` connects ABP's `IFeatureChecker` pipeline to Subscrio subscription entitlements.

```powershell
dotnet add package Subscrio.Abp
```

Register `Subscrio.Core` with the application's database connection, then choose one customer identity module.

Choose one customer identity module:

- `SubscrioAbpTenantModule` maps `ICurrentTenant.Id` to a Subscrio customer.
- `SubscrioAbpUserModule` maps `ICurrentUser.Id` to a Subscrio customer for applications that do not use tenants.

```csharp
[DependsOn(typeof(SubscrioAbpTenantModule))]
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
                    ConnectionString = "..."
                }
            },
            ServiceLifetime.Scoped);

        Configure<SubscrioAbpOptions>(options =>
        {
            options.ProductKey = "acme";
            options.IsManagedFeature = feature =>
                feature.Name.StartsWith("acme-", StringComparison.Ordinal);
        });
    }
}
```

Application code continues to use ABP:

```csharp
var reports = await featureChecker.IsEnabledAsync("acme-reports");
var maxProjects = await featureChecker.GetAsync<int>("acme-max-projects");
```

See the [public repository](https://github.com/subscrio/subscrio-abp) for the complete LocalDB sample, user-mode configuration, tests, and diagrams.
