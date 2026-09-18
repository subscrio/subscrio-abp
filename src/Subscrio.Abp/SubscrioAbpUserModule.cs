using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace Subscrio.Abp;

[DependsOn(typeof(SubscrioAbpModule))]
public sealed class SubscrioAbpUserModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<
            ISubscrioCustomerKeyResolver,
            CurrentUserSubscrioCustomerKeyResolver>();
    }
}
