# Subscrio for ABP

`Subscrio.Abp` connects ABP's `IFeatureChecker` to Subscrio subscription entitlements. ABP remains responsible for feature definitions and application-level checks. Subscrio resolves the current customer's value from its subscription, plan, and overrides.

Learn more at [subscrio.com](https://subscrio.com), or visit the main [subscrio/subscrio repository](https://github.com/subscrio/subscrio).

```powershell
dotnet add package Subscrio.Abp
```

Choose one customer identity module:

- `SubscrioAbpTenantModule` maps `ICurrentTenant.Id` to a Subscrio customer.
- `SubscrioAbpUserModule` maps `ICurrentUser.Id` to a Subscrio customer for applications that do not use tenants.

Register `Subscrio.Core` with the application's database connection and depend on the selected module:

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

See the [public repository](https://github.com/subscrio/subscrio-abp) for tenant and user configuration, catalog synchronization, the runnable sample, tests, and diagrams.
