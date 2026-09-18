using Microsoft.Extensions.Options;
using Subscrio.Core.Application.DTOs;
using Volo.Abp.Features;
using Volo.Abp.Validation.StringValues;

namespace Subscrio.Abp;

public sealed class AbpSubscrioFeatureCatalog
{
    private readonly IFeatureDefinitionManager _featureDefinitionManager;
    private readonly SubscrioAbpOptions _options;

    public AbpSubscrioFeatureCatalog(
        IFeatureDefinitionManager featureDefinitionManager,
        IOptions<SubscrioAbpOptions> options)
    {
        _featureDefinitionManager = featureDefinitionManager;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<FeatureConfig>> GetFeaturesAsync()
    {
        return (await _featureDefinitionManager.GetAllAsync())
            .Where(_options.IsManagedFeature)
            .OrderBy(feature => feature.Name, StringComparer.Ordinal)
            .Select(feature => new FeatureConfig(
                Key: _options.FeatureKeyMapper(feature.Name),
                DisplayName: _options.FeatureDisplayNameFactory(feature),
                Description: "Synced from the ABP feature definition at application initialization.",
                ValueType: GetSubscrioValueType(feature.ValueType),
                DefaultValue: feature.DefaultValue ?? "false",
                GroupName: _options.FeatureGroupName))
            .ToList();
    }

    private static string GetSubscrioValueType(IStringValueType? valueType)
    {
        if (valueType is null or ToggleStringValueType)
        {
            return "toggle";
        }

        return valueType.Validator is NumericValueValidator ? "numeric" : "text";
    }
}
