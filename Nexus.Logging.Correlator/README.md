# Nexus.Logging.Correlator #

## Overview ##

Logging Correlator provides Middleware components to correlate logs and service calls for requests by using correlation id.
Correlation id, parent correlation id and sequence are set/created automatically and gets included in every log entry. 
Also if HttpClientFactory is used, correlation id and sequence get passed to subsequent service calls automatically.

_This package does not provide any logging implementations._

## Installation and Registration ##

*Note:* Nexus.Logging should be configured in the solution to be able to use Correlator.


### Installation ###

Install latest version of ```Nexus.Logging.Correlator``` package from artifactory.

### Registration ###

Add following lines to appropriate sections in Startup.cs to register middleware components and required services for correlation

```csharp

        public void ConfigureServices(IServiceCollection services)
        {
            ...
            services.AddRequestCorrelation();
            ...
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            ...
            app.UseRequestCorrelation();
            ...
        }
```

### IHttpClientFactory ###

When using IHttpClientFactory registrations, each registration must be configured with the CorrelationMessageHandler which will automatically add the correlation context to outgoing requests.

```csharp
        public void ConfigureServices(IServiceCollection services)
        {
            ...
            services.AddHttpClient("Client1").AddCorrelationHandler();
            services.AddHttpClient("Client2").AddCorrelationHandler();
            ...
        }
```
*Note:* If you are not using IHttpClientFactory, you need to add correlation id (```x-pl-correlationid```) and sequence (```x-pl-sequence```) to outgoing request headers manually after incrementing sequence per outgoing request.

## Usage ##

There is no special usage directives. Whenever logger.Log(...) method is called, correlation id, parent correlation id and sequence for the request context is added to log properties which then can be searched on splunk to see particular request journey. 


