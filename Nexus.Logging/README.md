# Nexus.Logging #

Provides the generic implementation of `Nexus.Logging.ILogger` that 
allows for `ILoggerProvider` implementations to bind to the logging pipeline.

_This package does not provide any logging implementations._

## Usage ##

### Setup ###

The logger should be enabled and configured in the `Startup.ConfigureServices` method
by using the defined `AddNexusLogger` builder method.
```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddNexusLogger(_configuration, builder =>
    {
        // This is where the logger provider for each 
        // Nexus.Logging.* package is registered
        // Provider must implement Nexus.Logging.Configuration.IConfigureLoggerProvider.
        builder.RegisterLoggerProvider(new ConfigureInMemoryProvider());
        builder.RegisterLoggerProvider(new ConfigureSerilogProvider());
    });
    ...
}
```

`AddNexusLogger` requires the `IConfiguration` and an Action method for the `ILoggerBuilder` to 
configure the logging provider that should be used for the Targets.

**Sample appsettings.json:**
```json
{
    "ApplicationName": "{ReplaceWithAppName}",
    "AppSettings": {
        "ENV": "{ReplaceWithEnvironment}"
    },
    "Logging": {
    "MinimumLevel": "Warn",
    "Filters": {
      "Microsoft": "Error",
      "System": "Info"
    },
    "OutputTemplate": "[{Timestamp} {Level}] {Message}... or $formatter",
    "Targets": [
      {
        "Provider": "Serilog",
        "Type": "Console",
        "Args": {
          "ImAKey": "ImAValueForAKey"
        },
        "OutputTemplate": "TestingOverrideForTemplate"
      },
      {
        "Provider": "Serilog",
        "Type": "RollingFile",
        "Args": {
          "PathFormat": "C:\\Logs\\Example\\Path-{Date}.log",
          "FileCount": "15"
        }
      }
    ]
  }
}
```

*Refer to the `Nexus.Logging.Configuration` package README.md
and each logger provider implementation for more configuration details and available
options.*

---

## Using the logger ##

The logger should be used within the code base by using an injected dependency of 
`Nexus.Logging.Contract.ILogger<T>`:
```csharp
public class SomeClass
{
    private readonly ILogger<SomeClass> _logger;

    public SomeClass(ILogger<SomeClass> logger)
    {
        _logger = logger;
    }
}
```

---

### Log Message Templating ###

The logger supports message templates where replacement parameters in the message 
are enclosed in curly braces such as `{SomeId}`.  The values that should be used to replace the parameter 
must be added to *metadata* supplied to the `Log` method.
```csharp
_logger.Log(
  LogLevel.Debug, 
  "Logging a message for someId: {SomeId}",
  metadata: new Dictionary<string, object>
    {
       { "SomeId", "123" }
    });
```
*Note: The key value pairs that are submitted as metadata will also be included in the
`LogDetails` of the log message output.*

*Example Output*
```json
{
    ...
    "Message": "Logging a message for someId: 123"
    ...
    "LogDetails": {
      "SomeId": "123"
    }    
}
```

*Note: When the message template contains a parameter that has not been specified in the 
metadata values the parameter will be replaced with* `(null)`.

---
#### How are Type objects such as _classes_ and Anonymous Types serialized in the log message? ####

When the log message contains a template parameter that represents a class or anonymous 
type the following serialization rules apply:
- If the object is an Anonymous Type the value in the log message will always be 
serialized to its Json string representation.
- If the object is a _class_ with an overriden `ToString` method then `ToString` will be used.
- If the object is a _class_ without an overriden `ToString` then is will be serialized as a Json string.

---

#### Can log messages be preformatted with `string.Format` or `$""`? ####

The messages can be submitted preformatted to the `Log` method.
However, you will lose the benefit of the structured data in *metadata* flowing into
`LogDetails`.

It is recommended that the log message templating format is used in order to provide 
structured logging for simpler searching and indexing capabilities when using log 
aggregation services.

---

## Scopes ##

Use scope(s) to enrich log messages with relevant data points that can facilitate faster 
troubleshooting, metrics and log searching.

### Defining scopes: ###
```csharp
var myScopes = new Dictionary<string, object>{
    { "Key", "Value" },
    { "AnotherKey": "AnotherValue" }
};
```

---

### Preserving scope(s) through the call stack: ###

When you want to preserve scope(s) for subsequent log calls use `BeginScope` which 
will make the scope(s) available to every `Log` method that occurs within a using block.
```csharp
using(_logger.BeginScope(myScopes))
{
    _logger.Log(...)
    var result = _service.Get(123);
}
```
*Calls to `_logger.Log` within `_service.Get` will include values from `myScopes`.*

---

### Adding scope(s) for a single log message: ###

Custom scope(s) for a single log message are submitted as *metadata* on the `Log` method.
```csharp
_logger.Log(..., myScopes);
var result = _service.Get(123);

```
*Calls to `_logger.Log` within `_service.Get` will not include values from `myScopes`.*

---

### How do scope(s) appear in the log message? ###

Custom Scope(s) that are added using `BeginScope` or as *metadata* in the `Log` method
will be added as properties in `LogDetails` in the log message(s).  The structured logs 
will make it easier to search and index when using log aggregation tools.
```json
{
    ...
    "Message": "Something happened"
    ...
    "LogDetails": {
      "Key": "Value",
      "AnotherKey": "AnotherValue"
    }    
}
```

