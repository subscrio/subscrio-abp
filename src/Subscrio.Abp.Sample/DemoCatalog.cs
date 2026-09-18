namespace Subscrio.Abp.Sample;

public static class DemoCatalog
{
    public const string Version = "1";
    public const string FreeBillingCycleKey = "free-monthly";
    public const string ProBillingCycleKey = "pro-monthly";
    public const string SubscriptionKey = "acme-demo-subscription";

    public static readonly Guid TenantId = Guid.Parse("7f8d1c40-5f18-4ba8-879f-5f5e8e49d9f1");
}
