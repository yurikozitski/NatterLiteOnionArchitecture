using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog.Web;
using NatterLite_OA.Core.ServiceInterfaces;
using NatterLite_OA.Core.RepositoryInterfaces;

namespace NatterLite_OA.WebUI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

            try
            {
                var host = CreateHostBuilder(args).Build();
                using (var scope = host.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var userRepository = services.GetRequiredService<IUserRepository>();
                    var picturesProvider = services.GetRequiredService<IPicturesProvider>();
                    var dataInitializer = services.GetRequiredService<IDataInitializer>();
                    await dataInitializer.InitializeAsync(userRepository,picturesProvider);

                }

                host.Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occurred while seeding the database.");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Error);
                })
                .UseNLog();
    }
}
