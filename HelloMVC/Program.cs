using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
namespace HelloMVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webHost => webHost
                    .ConfigureServices(services => services
                        .AddRouting()
                        .AddControllersWithViews())
                .Configure(app => app
                    .UseRouting()
                    .UseEndpoints(endPoints => endPoints
                        .MapControllers()))
                )
                .Build()
                .Run();

        }
    }
}
