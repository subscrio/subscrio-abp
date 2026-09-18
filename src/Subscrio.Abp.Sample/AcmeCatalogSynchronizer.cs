using Subscrio.Core.Application.DTOs;
using SubscrioInstance = Subscrio.Core.Subscrio;

namespace Subscrio.Abp.Sample;

public sealed class AcmeCatalogSynchronizer
{
    private readonly AbpSubscrioFeatureCatalog _featureCatalog;

    public AcmeCatalogSynchronizer(AbpSubscrioFeatureCatalog featureCatalog)
    {
        _featureCatalog = featureCatalog;
    }

    public async Task SyncAsync(SubscrioInstance subscrio)
    {
        var features = await _featureCatalog.GetFeaturesAsync();

        if (features.Count != AppFeatures.All.Length)
        {
            throw new InvalidOperationException(
                $"Expected {AppFeatures.All.Length} ABP feature definitions, found {features.Count}.");
        }

        var config = new ConfigSyncDto(
            Version: DemoCatalog.Version,
            Features: features.ToList(),
            Products:
            [
                new ProductConfig(
                    Key: AppProducts.Acme,
                    DisplayName: "Acme",
                    Description: "The demo product used by the ABP and Subscrio integration sample.",
                    Features: features.Select(feature => feature.Key).ToList(),
                    Plans:
                    [
                        new PlanConfig(
                            Key: AppPlans.Free,
                            DisplayName: "Free",
                            Description: "The default Acme plan.",
                            FeatureValues: new Dictionary<string, string>
                            {
                                [AppFeatures.Reports] = "false",
                                [AppFeatures.MaxProjects] = "3"
                            },
                            BillingCycles:
                            [
                                new BillingCycleConfig(
                                    Key: DemoCatalog.FreeBillingCycleKey,
                                    DisplayName: "Free monthly",
                                    DurationValue: 1,
                                    DurationUnit: "months")
                            ]),
                        new PlanConfig(
                            Key: AppPlans.Pro,
                            DisplayName: "Pro",
                            Description: "The paid Acme plan.",
                            FeatureValues: new Dictionary<string, string>
                            {
                                [AppFeatures.Reports] = "true",
                                [AppFeatures.MaxProjects] = "100"
                            },
                            BillingCycles:
                            [
                                new BillingCycleConfig(
                                    Key: DemoCatalog.ProBillingCycleKey,
                                    DisplayName: "Pro monthly",
                                    DurationValue: 1,
                                    DurationUnit: "months")
                            ])
                    ])
            ]);

        var report = await subscrio.ConfigSync.SyncFromJsonAsync(config);

        if (report.Errors.Count > 0)
        {
            throw new InvalidOperationException(
                "Subscrio catalog sync failed: " +
                string.Join("; ", report.Errors.Select(error => $"{error.EntityType}/{error.Key}: {error.Message}")));
        }

        Console.WriteLine(
            "Catalog sync: " +
            $"created F/P/Pl/B {report.Created.Features}/{report.Created.Products}/{report.Created.Plans}/{report.Created.BillingCycles}; " +
            $"updated F/P/Pl/B {report.Updated.Features}/{report.Updated.Products}/{report.Updated.Plans}/{report.Updated.BillingCycles}.");
    }
}
