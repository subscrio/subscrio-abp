using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Subscrio.Abp.Sample;

public static class AppFeatures
{
    public const string Reports = "acme-reports";
    public const string MaxProjects = "acme-max-projects";

    public static readonly string[] All = [Reports, MaxProjects];
}

public static class AppPlans
{
    public const string Free = "free";
    public const string Pro = "pro";
}

public static class AppProducts
{
    public const string Acme = "acme";
}

public static class AppTenants
{
    public static string GetCustomerKey(Guid tenantId) => $"tenant-{tenantId:N}";
}

public sealed class AcmeFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(
            "Acme",
            new FixedLocalizableString("Acme"));

        group.AddFeature(
            AppFeatures.Reports,
            defaultValue: "false",
            displayName: new FixedLocalizableString("Reports"),
            valueType: new ToggleStringValueType());

        group.AddFeature(
            AppFeatures.MaxProjects,
            defaultValue: "3",
            displayName: new FixedLocalizableString("Maximum projects"),
            valueType: new FreeTextStringValueType(new NumericValueValidator(0, 10000)));
    }
}
