using SubscrioInstance = Subscrio.Core.Subscrio;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;

namespace Subscrio.Abp.Sample;

public sealed class DemoRunner
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IFeatureChecker _featureChecker;
    private readonly IFeatureDefinitionManager _featureDefinitionManager;
    private readonly SubscrioInstance _subscrio;

    public DemoRunner(
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        IFeatureDefinitionManager featureDefinitionManager,
        SubscrioInstance subscrio)
    {
        _currentTenant = currentTenant;
        _featureChecker = featureChecker;
        _featureDefinitionManager = featureDefinitionManager;
        _subscrio = subscrio;
    }

    public async Task RunAsync()
    {
        var reportsDefinition = await _featureDefinitionManager.GetAsync(AppFeatures.Reports);
        var maxProjectsDefinition = await _featureDefinitionManager.GetAsync(AppFeatures.MaxProjects);
        var defaultReports = bool.Parse(reportsDefinition.DefaultValue ?? "false");
        var defaultMaxProjects = int.Parse(maxProjectsDefinition.DefaultValue ?? "0");

        var planValues = (await _subscrio.Plans.GetPlanFeaturesAsync(AppPlans.Pro))
            .ToDictionary(value => value.FeatureKey, value => value.Value, StringComparer.Ordinal);
        var planReports = bool.Parse(planValues[AppFeatures.Reports]);
        var planMaxProjects = int.Parse(planValues[AppFeatures.MaxProjects]);

        var customerKey = AppTenants.GetCustomerKey(DemoCatalog.TenantId);
        var customerReports = await _subscrio.FeatureChecker.IsEnabledForCustomerAsync(
            customerKey,
            AppProducts.Acme,
            AppFeatures.Reports);
        var customerMaxProjects = await _subscrio.FeatureChecker.GetValueForCustomerAsync<int>(
            customerKey,
            AppProducts.Acme,
            AppFeatures.MaxProjects);

        using (_currentTenant.Change(DemoCatalog.TenantId, "Acme Manufacturing"))
        {
            var reports = await _featureChecker.IsEnabledAsync(AppFeatures.Reports);
            var maxProjects = await _featureChecker.GetAsync<int>(AppFeatures.MaxProjects);

            Ensure(!defaultReports, AppFeatures.Reports + " default", "false");
            Ensure(defaultMaxProjects == 3, AppFeatures.MaxProjects + " default", "3");
            Ensure(planReports, AppFeatures.Reports + " plan value", "true");
            Ensure(planMaxProjects == 100, AppFeatures.MaxProjects + " plan value", "100");
            Ensure(customerReports, AppFeatures.Reports + " customer value", "true");
            Ensure(customerMaxProjects == 250, AppFeatures.MaxProjects + " customer value", "250");
            Ensure(reports, AppFeatures.Reports, "true");
            Ensure(maxProjects == 250, AppFeatures.MaxProjects, "250");

            Console.WriteLine();
            Console.WriteLine("1. FEATURE DEFAULTS");
            Console.WriteLine("────────────────────────────");
            Console.WriteLine($"Reports:       {defaultReports.ToString().ToLowerInvariant()}");
            Console.WriteLine($"MaxProjects:   {defaultMaxProjects}");
            Console.WriteLine();
            Console.WriteLine("2. PRO PLAN VALUES");
            Console.WriteLine("────────────────────────────");
            Console.WriteLine($"Reports:       {planReports.ToString().ToLowerInvariant()}");
            Console.WriteLine($"MaxProjects:   {planMaxProjects}");
            Console.WriteLine();
            Console.WriteLine("3. CUSTOMER VALUES");
            Console.WriteLine("────────────────────────────");
            Console.WriteLine("Customer:      Acme Manufacturing");
            Console.WriteLine("Plan:          Pro");
            Console.WriteLine();
            Console.WriteLine($"Reports:       {customerReports.ToString().ToLowerInvariant()}");
            Console.WriteLine($"MaxProjects:   {customerMaxProjects}   (customer override)");
            Console.WriteLine();
            Console.WriteLine("Resolved through ABP IFeatureChecker:");
            Console.WriteLine($"Reports:       {reports.ToString().ToLowerInvariant()}");
            Console.WriteLine($"MaxProjects:   {maxProjects}");
            Console.WriteLine();
            Console.WriteLine("PASS: defaults → plan → customer override resolved correctly.");
        }
    }

    private static void Ensure(bool condition, string featureName, string expected)
    {
        if (!condition)
        {
            throw new InvalidOperationException(
                $"Feature '{featureName}' did not resolve to expected value '{expected}'.");
        }
    }
}
