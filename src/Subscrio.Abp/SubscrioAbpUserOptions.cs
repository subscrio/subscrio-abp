namespace Subscrio.Abp;

public sealed class SubscrioAbpUserOptions
{
    public Func<Guid, string> CustomerKeyFactory { get; set; } =
        userId => $"user-{userId:N}";
}
