using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Autofac;

namespace Subscrio.Abp.Sample;

public static class Program
{
    public static async Task<int> Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        try
        {
            using var application = await AbpApplicationFactory.CreateAsync<AbpSubscrioSampleModule>(options =>
            {
                options.UseAutofac();
            });

            await application.InitializeAsync();
            await application.ServiceProvider.GetRequiredService<DemoRunner>().RunAsync();
            await application.ShutdownAsync();

            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Demo failed: {exception.Message}");
            return 1;
        }
    }
}
