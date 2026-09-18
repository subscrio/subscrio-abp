using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Features;
using Volo.Abp.Modularity;

namespace Subscrio.Abp;

[DependsOn(typeof(AbpFeaturesModule))]
public sealed class SubscrioAbpModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<SubscrioFeatureValueProvider>();
        context.Services.AddTransient<AbpSubscrioFeatureCatalog>();

        Configure<AbpFeatureOptions>(options =>
        {
            options.ValueProviders.Add<SubscrioFeatureValueProvider>();
        });
    }
}
