using Microsoft.Extensions.Options;
using Volo.Abp.Users;

namespace Subscrio.Abp;

public sealed class CurrentUserSubscrioCustomerKeyResolver : ISubscrioCustomerKeyResolver
{
    private readonly ICurrentUser _currentUser;
    private readonly SubscrioAbpUserOptions _options;

    public CurrentUserSubscrioCustomerKeyResolver(
        ICurrentUser currentUser,
        IOptions<SubscrioAbpUserOptions> options)
    {
        _currentUser = currentUser;
        _options = options.Value;
    }

    public string? GetCustomerKeyOrNull()
    {
        return _currentUser.Id.HasValue
            ? _options.CustomerKeyFactory(_currentUser.Id.Value)
            : null;
    }
}
