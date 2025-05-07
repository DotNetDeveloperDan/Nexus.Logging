using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using Nexus.Logging.Correlator.Extensions;
using Nexus.Logging.Serilog;

namespace Nexus.Logging.Harness.Service2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddNexusLogger(Configuration, builder => { builder.RegisterSerilog(); });
            services.AddRequestCorrelation();
            services.AddHttpClient("ThirdService",
                    httpClient =>
                    {
                        httpClient.BaseAddress =
                            new Uri("https://Nexus.logging.testharness.service3");
                    }) // When running this in a Linux Docker container the kestrel server cannot validate the certificate and fails. This circumvents the validation so calls still work.
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true }) //NOSONAR
                .AddCorrelationHandler();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRequestCorrelation();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
