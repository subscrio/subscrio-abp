using Microsoft.Extensions.Options;
using Volo.Abp.MultiTenancy;

namespace Subscrio.Abp;

public sealed class CurrentTenantSubscrioCustomerKeyResolver : ISubscrioCustomerKeyResolver
{
    private readonly ICurrentTenant _currentTenant;
    private readonly SubscrioAbpTenantOptions _options;

    public CurrentTenantSubscrioCustomerKeyResolver(
        ICurrentTenant currentTenant,
        IOptions<SubscrioAbpTenantOptions> options)
    {
        _currentTenant = currentTenant;
        _options = options.Value;
    }

    public string? GetCustomerKeyOrNull()
    {
        return _currentTenant.Id.HasValue
            ? _options.CustomerKeyFactory(_currentTenant.Id.Value)
            : null;
    }
}
