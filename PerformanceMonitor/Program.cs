using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace PerformanceMonitor
{
    internal class Program
    {
        static void Main(string[] args)
        {
           new HostBuilder()
                .ConfigureServices(svcs => svcs
                .AddHostedService<PerformanceMetricsCollectorWithDI>()
                .AddSingleton<IProcessMetricsCollector,FakeMetricsCollector>()
                .AddSingleton<INetworkMetricsCollector,FakeMetricsCollector>()
                .AddSingleton<IMemoryMetricsCollector,FakeMetricsCollector>()
                .AddSingleton<FakeMetricsDelivery>()
                .AddSingleton<IMetricsDelivery,FakeMetricsDeliveryNew>()
                )
                .Build()
                .Run();
        }
    }
}
