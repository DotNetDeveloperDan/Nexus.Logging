using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net.Http;
using Nexus.Logging.Correlator.Extensions;

namespace Nexus.Logging.Harness.IntegrationTests
{
    public static class Initializer
    {
        private static readonly string BaseDirectory = Utilities.GetSolutionDirectory();

        /// <summary>
        /// Sets up the call chain in memory host and handlers.
        /// </summary>
        /// <returns></returns>
        public static HttpClient InitializeChain()
        {
            var h3 = InitService3();
            var noCor = InitServiceNoCorrelator(h3);
            var h2 = InitService2(h3);
            return InitTestHarness(h2, noCor);
        }

        /// <summary>
        /// Service3 is the last in the chain
        /// </summary>
        /// <returns></returns>
        public static HttpMessageHandler InitService3()
        {
            var appsettingsPath = Path.Combine(BaseDirectory, "Nexus.Logging.Harness.Service3/appsettings.json");

            var config = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile(appsettingsPath, optional: true, reloadOnChange: true)
               .AddEnvironmentVariables()
               .Build();

            var builder = new WebHostBuilder()
                .UseConfiguration(config)
                .UseStartup<Service3.Startup>();

            var server = new TestServer(builder);
            return server.CreateHandler();
        }

        /// <summary>
        /// Dependency on Service3
        /// </summary>
        /// <param name="chainableHandler"></param>
        /// <returns></returns>
        public static HttpMessageHandler InitServiceNoCorrelator(HttpMessageHandler chainableHandler)
        {
            var appsettingsPath = Path.Combine(BaseDirectory, "Nexus.Logging.Harness.ServiceNoCorrelator/appsettings.json");

            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(appsettingsPath, optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var builder = new WebHostBuilder()
                .UseConfiguration(config)
                .UseStartup<ServiceNoCorrelator.Startup>()
                .ConfigureTestServices(services =>
                {
                    services.AddHttpClient("ThirdService").ConfigurePrimaryHttpMessageHandler(_ => chainableHandler);

                });

            var server = new TestServer(builder);
            return server.CreateHandler();
        }


        /// <summary>
        /// Dependency on Service3
        /// </summary>
        /// <param name="chainableHandler"></param>
        /// <returns></returns>
        public static HttpMessageHandler InitService2(HttpMessageHandler chainableHandler)
        {
            var appsettingsPath = Path.Combine(BaseDirectory, "Nexus.Logging.Harness.Service2/appsettings.json");

            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(appsettingsPath, optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var builder = new WebHostBuilder()
                .UseConfiguration(config)
                .UseStartup<Service2.Startup>()
                .ConfigureTestServices(services =>
                {
                    services.AddHttpClient("ThirdService").ConfigurePrimaryHttpMessageHandler(_ => chainableHandler).AddCorrelationHandler();

                });

            var server = new TestServer(builder);
            return server.CreateHandler();
        }

        /// <summary>
        /// Consumer of all.
        /// </summary>
        /// <param name="secondServiceHandler"></param>
        /// <param name="noCorrelatorHandler"></param>
        /// <returns></returns>
        public static HttpClient InitTestHarness(HttpMessageHandler secondServiceHandler, HttpMessageHandler noCorrelatorHandler)
        {
            var appsettingsPath = Path.Combine(BaseDirectory, "Nexus.Logging.Harness.Service1/appsettings.json");

            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(appsettingsPath, optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var builder = new WebHostBuilder()
                .UseConfiguration(config)
                .UseStartup<Service1.Startup>()
                .ConfigureTestServices(services =>
                {
                    services.AddHttpClient("SecondService").ConfigurePrimaryHttpMessageHandler(_ => secondServiceHandler).AddCorrelationHandler();
                    services.AddHttpClient("ServiceNoCorrelator").ConfigurePrimaryHttpMessageHandler(_ => noCorrelatorHandler);

                });

            var server = new TestServer(builder);
            var client = server.CreateClient();
            return client;
        }
    }
}
