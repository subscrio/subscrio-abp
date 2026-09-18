using Microsoft.Extensions.Options;
using SubscrioInstance = Subscrio.Core.Subscrio;
using Volo.Abp.Features;

namespace Subscrio.Abp;

public sealed class SubscrioFeatureValueProvider : IFeatureValueProvider
{
    private readonly SubscrioInstance _subscrio;
    private readonly ISubscrioCustomerKeyResolver _customerKeyResolver;
    private readonly SubscrioAbpOptions _options;

    public SubscrioFeatureValueProvider(
        SubscrioInstance subscrio,
        IEnumerable<ISubscrioCustomerKeyResolver> customerKeyResolvers,
        IOptions<SubscrioAbpOptions> options)
    {
        var resolvers = customerKeyResolvers.ToList();

        if (resolvers.Count != 1)
        {
            throw new InvalidOperationException(
                resolvers.Count == 0
                    ? $"Depend on {nameof(SubscrioAbpTenantModule)} or {nameof(SubscrioAbpUserModule)}, " +
                      $"or register one {nameof(ISubscrioCustomerKeyResolver)} implementation."
                    : $"Found {resolvers.Count} {nameof(ISubscrioCustomerKeyResolver)} registrations. " +
                      "Choose exactly one customer identity strategy.");
        }

        _subscrio = subscrio;
        _customerKeyResolver = resolvers[0];
        _options = options.Value;
    }

    public string Name => "Subscrio";

    public async Task<string?> GetOrNullAsync(FeatureDefinition feature)
    {
        if (!_options.IsManagedFeature(feature))
        {
            return null;
        }

        var customerKey = _customerKeyResolver.GetCustomerKeyOrNull();

        if (customerKey is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(customerKey))
        {
            throw new InvalidOperationException(
                $"{nameof(ISubscrioCustomerKeyResolver)} returned an empty customer key.");
        }

        if (string.IsNullOrWhiteSpace(_options.ProductKey))
        {
            throw new InvalidOperationException(
                $"Configure {nameof(SubscrioAbpOptions)}.{nameof(SubscrioAbpOptions.ProductKey)} " +
                "before resolving Subscrio-backed ABP features.");
        }

        var featureKey = _options.FeatureKeyMapper(feature.Name);

        if (string.IsNullOrWhiteSpace(featureKey))
        {
            throw new InvalidOperationException(
                $"{nameof(SubscrioAbpOptions)}.{nameof(SubscrioAbpOptions.FeatureKeyMapper)} " +
                $"returned an empty key for ABP feature '{feature.Name}'.");
        }

        return await _subscrio.FeatureChecker.GetValueForCustomerAsync<string>(
            customerKey,
            _options.ProductKey,
            featureKey);
    }
}
