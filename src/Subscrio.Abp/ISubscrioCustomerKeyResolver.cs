namespace Subscrio.Abp;

public interface ISubscrioCustomerKeyResolver
{
    string? GetCustomerKeyOrNull();
}
