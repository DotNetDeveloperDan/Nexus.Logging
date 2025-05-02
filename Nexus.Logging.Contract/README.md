### Nexus.Logging.Contract ###

Provides abstractions that are commonly used with dependency injection for types such as:
- ILogger
- ILogger&lt;T&gt;
- LogLevel

_This package does not provide any logging implementations._

##### Logging #####

The `ILogger` provides a single `Log` method.

```csharp
void Log(
  LogLevel level,
  string message,
  Exception exception = null,
  IDictionary<string, object> metadata = null);
```

###### LogLevel ######

The available log levels are:
- Info
- Debug
- Warn
- Error 
- Fatal

###### Metadata ######

Custom metadata classes can be created by extending the `LogMetadataBase` class.
Each property should be configured with a getter that uses `LogMetadataBase.GetValue`
and a setter that uses `LogMetadataBase.SetValue`.

*Example implementation:*
```csharp
public class LogDetails : LogMetadataBase
{
  public string LeaseId
  {
    get { return GetValue(); }
    set { SetValue(value); }
  }

  public string StoreId
  {
    get { return GetValue(); }
    set { SetValue(value); }
  }
}
```
```csharp
_logger.Log(
  LogLevel.Info,
  "example message",
  metadata: new LogDetails(){ LeaseId = "987987" });
```

*NOTE: If a property value is not set in the implementation it will not be present in logs.*

