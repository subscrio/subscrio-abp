namespace Subscrio.Abp;

public sealed class SubscrioAbpTenantOptions
{
    public Func<Guid, string> CustomerKeyFactory { get; set; } =
        tenantId => $"tenant-{tenantId:N}";
}
