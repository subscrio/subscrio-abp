using Microsoft.Extensions.DependencyInjection;
using Subscrio.Core.Config;
using Subscrio.Core.DependencyInjection;
using Subscrio.Core.Domain.ValueObjects;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Subscrio.Abp.Sample;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(SubscrioAbpTenantModule)
)]
public sealed class AbpSubscrioSampleModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSubscrio(
            new SubscrioConfig
            {
                Database = new DatabaseConfig
                {
                    ConnectionString = LocalDbDatabase.ConnectionString,
                    DatabaseType = DatabaseType.SqlServer,
                    Ssl = false
                }
            },
            ServiceLifetime.Scoped);

        context.Services.AddTransient<AcmeCatalogSynchronizer>();
        context.Services.AddTransient<DemoDataSeeder>();
        context.Services.AddTransient<DemoRunner>();

        Configure<SubscrioAbpOptions>(options =>
        {
            options.ProductKey = AppProducts.Acme;
            options.FeatureGroupName = "Acme";
            options.IsManagedFeature = feature =>
                AppFeatures.All.Contains(feature.Name, StringComparer.Ordinal);
            options.FeatureDisplayNameFactory = feature => feature.Name switch
            {
                AppFeatures.Reports => "Reports",
                AppFeatures.MaxProjects => "Maximum projects",
                _ => feature.Name
            };
        });

        Configure<SubscrioAbpTenantOptions>(options =>
        {
            options.CustomerKeyFactory = AppTenants.GetCustomerKey;
        });
    }

    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await LocalDbDatabase.EnsureCreatedAsync();

        var subscrio = context.ServiceProvider.GetRequiredService<Subscrio.Core.Subscrio>();

        if (await subscrio.VerifySchemaAsync() is null)
        {
            await subscrio.InstallSchemaAsync();
        }

        await subscrio.MigrateAsync();

        var synchronizer = context.ServiceProvider.GetRequiredService<AcmeCatalogSynchronizer>();
        await synchronizer.SyncAsync(subscrio);

        var seeder = context.ServiceProvider.GetRequiredService<DemoDataSeeder>();
        await seeder.SeedAsync(subscrio);
    }
}
