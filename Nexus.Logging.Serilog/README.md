# Nexus.Logging.Serilog #

Provides configuration of Serilog as the logging Provider for `Nexus.Logging.ILogger`.

_This package is a logging implementation based on Serilog._

## Usage ##

## Setup ##

## Configuration ##

- _OutputTemplate_: Defines the default output format.
  - Predefined formatters are used by prefixing the formatter name with a `$` symbol
  such as `$NexusJsonLogFormatter`.
  - If the `$` prefix is not included it will be interpretated as a template.
  - If the _Target_ includes an _OutputTemplate_ parameter it will override the default
  format for the specific _Target_.
- _Target:Provider_: Used to specify the _Provider_ that should be used with the _Target_.
  - Use `Serilog` as the _Provider_ in order to use Serilog Sinks for the _Target_.

See [ConfigureOptionsSchema.json](/ConfigureOptionsSchema.json) for more information about the 
configuration options available.

_Sample appsettings.json:_
```json
{
    "ApplicationName": "{ReplaceWithAppName}",
    "AppSettings": {
      "ENV": "{ReplaceWithEnvironment}"
    },
    "Logging": {
      "MinimumLevel": "Info",
      "Filters": {
        "Microsoft": "Error",
        "System": "Error"
      },
      "OutputTemplate": "$NexusJsonLogFormatter",
      "Targets": [
        {
          "Provider": "Serilog",
          "Type": "Console"
        },
        {
          "Provider": "Serilog",
          "Type": "RollingFile",
          "Args": {
            "PathFormat": "D:\\NexusAppLogs\\{Environment}\\{ApplicationName}_{Date}.log",
            "FileCount": "15",
            "FileSize": "{SizeInBytes}",
            "MinimumLevel": "{IncludeIfYouWantToOverrideGlobalLevel}"
        }
      }
    ]
  }
}
```

## Logger Registration in `Startup.cs` ##

The use of Serilog as the `ILoggerProvider` is configured through the use of `RegisterSerilog` in the builder action method of `AddNexusLogger`.

The registration process uses the details of the `Logging` configuration section of `AppSettings.json` to configure the sinks that should be used.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddNexusLogger(_configuration, builder =>
    {
        // Register the IConfigureLoggerProvider implementation for Serilog 
        builder.RegisterSerilog();
    });
    ...
}
```

---

## Advanced Configuration ##

### Service Targets ###

The Target Type of Service is used to configure additional Serilog Sinks beyond Console, Debug and RollingFile.

There are additional configuration items (Args) that are required to register the Target during the configuration process.

- _Using:_ Defines the namespace for the sink.
- _Name:_ The name of the configuration method that should be used to setup the sink.

In the case of the `Serilog.Sinks.MSSqlServer` additional parameters, required or optional are needed to configure the sink. The additional parameters should be passed in the Target Args with property names that match the parameter name (not case sensitive).

_Example Configuration:_

```json
...
  "Provider": "Serilog",
  "Type": "Service",
  "Args": {
    "Using": "Serilog.Sinks.MSSqlServer",
    "Name": "MSSqlServer",
    "ConnectionString": "",
    "TableName": "",
    ...
  }
...
```

---

## Logging ##

The configuration process will register the `Nexus.Logging.Contract.ILogger<T>` with dependency injection and connect the logging targets to the provider.

*Note: Make sure that you are referencing the `ILogger<T>` from Nexus.Logging.Contract and __not__ Microsoft.Extensions.Logging.*

```csharp
private readonly ILogger<T> _logger;

public SomeClass(ILogger<T> logger)
{
    _logger = logger;_
}

public void DoSomething(string someId)
{
    _logger.Log(LogLevel.Debug, "Logging a message");
}
```

### Templated Messages ###

The `ILogger` supports message templates where the parameters that should be replaced are enclosed in curly braces. The parameter name in the template must match the key in the metadata.

```csharp
_logger.Log(
    LogLevel.Debug, 
    "Logging a message for someId: {SomeId}",
    metadata: new Dictionary<string, object>
      {
         { "SomeId", "123" }
      });
```

It is recommended that templated messages with metadata are used rather than formatting the message using `$` or `string.Format` since the metadata submitted to the logger will also be included as structured data in the log message.

```json
{
  ...
  "Message": "Logging a message for someId: 123",
  "LogDetails": {
    "SomeId": "123"
  }
  ...
}
```

---
### Controlling Scope Formatting in LogDetails ###

When scopes are included in the log entries either through the use of `BeginScope` or as _metadata_ on an individual log message some general formatting rules will apply.

When a scalar type is included it will be serialized to the Json representation of that type.

- string -> `"value"`
- bool -> `true` or `false`
- int -> `value`
- ...

When using complex types such as a class, struct or anonymous type it can be serialized into JSON by including an `@` as the prefix for the _metadata_ key for that item. Without the prefix of `@` it will be serialized using the available `ToString` method of the type.

> Remember when using Message Templates that the metadata key must match the template
> key exactly to format the _Message_ portion of the log entry.  If the `@` is included
> in the metadata key then it must be included in the template key.

Examples:

#### If class Foo does not explicitly override ToString ####

```csharp
public class Foo
{
    public string Bar { get; set; }
    public bool Baz { get; set; }
}

_logger.Log(
    LogLevel.Debug, 
    "Logging a message for someId: {SomeId}",
    metadata: new Dictionary<string, object>
      {
         { "Foo", new NexusLeasing.Foo(){ Bar = "Test", Baz = true } }
      });

// Serializes with the generic inherited object.ToString
{
  ...
  "LogDetails": {
    "Foo": "NexusLeasing.Foo"
  }
  ...
}
```

#### If class Foo has an explicit override of ToString ####

```csharp

public class Foo
{
    public string Bar { get; set; }
    public bool Baz { get; set; }
    
    public override string ToString()
    {
        return $"Foo.Bar={Bar};Foo.Baz={Baz}";
    }
}

_logger.Log(
    LogLevel.Debug, 
    "Logging a message for someId: {SomeId}",
    metadata: new Dictionary<string, object>
      {
         { "Foo", new NexusLeasing.Foo(){ Bar = "Test", Baz = true } }
      });

// Serializes using the override of ToString
{
  ...
  "LogDetails": {
    "Foo": "Foo.Bar=Test;Foo.Baz=True"
  }
  ...
}
```

#### When the @ prefix is included on the metadata key ####

```csharp
public class Foo
{
    public string Bar { get; set; }
    public bool Baz { get; set; }
}

_logger.Log(
    LogLevel.Debug, 
    "Logging a message for someId: {SomeId}",
    metadata: new Dictionary<string, object>
      {
         { "@Foo", new NexusLeasing.Foo(){ Bar = "Test", Baz = true } }
      });

// Serializes as the JSON representation of Foo
{
  ...
  "LogDetails": {
    "Foo": {
        "Bar": "Test",
        "Baz": true
    }
  }
  ...
}

```

---

For more details on usage refer to [Nexus.Logging](https://bitbucket.org/Nexusfin-ondemand/Nexus.logging/src/master/Nexus.Logging/README.md).

## Splunk and Timestamp ##

When searching in Splunk you should use `_time`, not `Timestamp`. `Timestamp` from the logs is ingested as `_time`. `_time` is used internally by Splunk and converted to local time as needed. https://docs.splunk.com/Documentation/Splunk/8.1.1/SearchReference/Commontimeformatvariables

