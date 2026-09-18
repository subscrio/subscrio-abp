using Subscrio.Core.Application.DTOs;
using Subscrio.Core.Domain.ValueObjects;
using SubscrioInstance = Subscrio.Core.Subscrio;

namespace Subscrio.Abp.Sample;

public sealed class DemoDataSeeder
{
    public async Task SeedAsync(SubscrioInstance subscrio)
    {
        var customerKey = AppTenants.GetCustomerKey(DemoCatalog.TenantId);

        var customer = await subscrio.Customers.GetCustomerAsync(customerKey);

        if (customer is null)
        {
            await subscrio.Customers.CreateCustomerAsync(new CreateCustomerDto(
                Key: customerKey,
                DisplayName: "Acme Manufacturing",
                Email: "owner@example.test",
                Metadata: new Dictionary<string, object?>
                {
                    ["abpTenantId"] = DemoCatalog.TenantId.ToString("D")
                }));
        }
        else if (customer.DisplayName != "Acme Manufacturing")
        {
            await subscrio.Customers.UpdateCustomerAsync(
                customerKey,
                new UpdateCustomerDto(DisplayName: "Acme Manufacturing"));
        }

        var subscription = await subscrio.Subscriptions.GetSubscriptionAsync(DemoCatalog.SubscriptionKey);

        if (subscription is null)
        {
            await subscrio.Subscriptions.CreateSubscriptionAsync(new CreateSubscriptionDto(
                Key: DemoCatalog.SubscriptionKey,
                CustomerKey: customerKey,
                BillingCycleKey: DemoCatalog.ProBillingCycleKey,
                ActivationDate: DateTime.UtcNow,
                CurrentPeriodStart: DateTime.UtcNow,
                CurrentPeriodEnd: DateTime.UtcNow.AddMonths(1)));
        }
        else if (subscription.BillingCycleKey != DemoCatalog.ProBillingCycleKey)
        {
            await subscrio.Subscriptions.UpdateSubscriptionAsync(
                DemoCatalog.SubscriptionKey,
                new UpdateSubscriptionDto(BillingCycleKey: DemoCatalog.ProBillingCycleKey));
        }

        await subscrio.Subscriptions.AddFeatureOverrideAsync(
            DemoCatalog.SubscriptionKey,
            AppFeatures.MaxProjects,
            "250",
            OverrideType.Permanent);
    }
}
