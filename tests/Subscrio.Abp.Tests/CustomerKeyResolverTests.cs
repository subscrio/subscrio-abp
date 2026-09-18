using Microsoft.Extensions.Options;
using NSubstitute;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using Xunit;

namespace Subscrio.Abp.Tests;

public sealed class CustomerKeyResolverTests
{
    [Fact]
    public void Feature_provider_requires_a_customer_resolver()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            new SubscrioFeatureValueProvider(
                null!,
                [],
                Options.Create(new SubscrioAbpOptions())));

        Assert.Contains("register one", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Feature_provider_rejects_multiple_customer_resolvers()
    {
        ISubscrioCustomerKeyResolver[] resolvers =
        [
            new FixedCustomerKeyResolver("tenant-one"),
            new FixedCustomerKeyResolver("user-one")
        ];

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new SubscrioFeatureValueProvider(
                null!,
                resolvers,
                Options.Create(new SubscrioAbpOptions())));

        Assert.Contains("exactly one", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Tenant_resolver_uses_current_tenant_id()
    {
        var tenantId = Guid.Parse("6212ef84-4c32-4149-b034-9f610185986d");
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.Id.Returns(tenantId);

        var resolver = new CurrentTenantSubscrioCustomerKeyResolver(
            currentTenant,
            Options.Create(new SubscrioAbpTenantOptions()));

        Assert.Equal($"tenant-{tenantId:N}", resolver.GetCustomerKeyOrNull());
    }

    [Fact]
    public void Tenant_resolver_returns_null_without_a_current_tenant()
    {
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.Id.Returns((Guid?)null);

        var resolver = new CurrentTenantSubscrioCustomerKeyResolver(
            currentTenant,
            Options.Create(new SubscrioAbpTenantOptions()));

        Assert.Null(resolver.GetCustomerKeyOrNull());
    }

    [Fact]
    public void User_resolver_uses_current_user_id()
    {
        var userId = Guid.Parse("994eea63-7eaf-46ba-930f-fdcd5f762ac0");
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.Id.Returns(userId);

        var resolver = new CurrentUserSubscrioCustomerKeyResolver(
            currentUser,
            Options.Create(new SubscrioAbpUserOptions()));

        Assert.Equal($"user-{userId:N}", resolver.GetCustomerKeyOrNull());
    }

    [Fact]
    public void User_resolver_returns_null_without_an_authenticated_user()
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.Id.Returns((Guid?)null);

        var resolver = new CurrentUserSubscrioCustomerKeyResolver(
            currentUser,
            Options.Create(new SubscrioAbpUserOptions()));

        Assert.Null(resolver.GetCustomerKeyOrNull());
    }

    private sealed class FixedCustomerKeyResolver(string customerKey)
        : ISubscrioCustomerKeyResolver
    {
        public string? GetCustomerKeyOrNull() => customerKey;
    }
}
