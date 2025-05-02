
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Nexus.Logging.Configuration;
using Nexus.Logging.Contract;

namespace Nexus.Logging
{
    /// <summary>
    /// Builder extensions to configure logging in the <see cref="IServiceCollection"/>.
    /// </summary>
    public static class LoggerServiceCollectionExtensions
    {
        /// <summary>
        /// Adds and configures Nexus.Logging services in the <see cref="IServiceCollection"/>.
        /// <para>Configures the <see cref="ILoggerBuilder" />.</para>
        /// <para>Registers the <see cref="IGlobalScope"/> log context.</para>
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the logging services to.</param>
        /// <param name="configuration"></param>
        /// <param name="configureBuilder"></param>
        /// <returns>The <see cref="IServiceCollection"/> for additional call chaining.</returns>
        public static IServiceCollection AddNexusLogger(this IServiceCollection services, IConfiguration configuration, Action<ILoggerBuilder> configureBuilder)
        {
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            
            var loggerBuilder = services.ConfigureLogging(configuration);
            configureBuilder(loggerBuilder);
            
            services.AddSingleton(provider => new ApplicationScope(provider.GetRequiredService<ApplicationScopeOptions>()));
            
            return loggerBuilder.Build();
        }

    }
}
