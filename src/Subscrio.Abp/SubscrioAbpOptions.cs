using Volo.Abp.Features;

namespace Subscrio.Abp;

public sealed class SubscrioAbpOptions
{
    public string ProductKey { get; set; } = string.Empty;

    public string FeatureGroupName { get; set; } = "ABP";

    public Func<FeatureDefinition, bool> IsManagedFeature { get; set; } = _ => false;

    public Func<string, string> FeatureKeyMapper { get; set; } = featureName => featureName;

    public Func<FeatureDefinition, string> FeatureDisplayNameFactory { get; set; } =
        feature => feature.Name;
}
