### Nexus.Logging.Configuration ###

Provides uniform configuration options and auto-binding for appsettings.json based configuration.
Provides contract for registering logging providers with the `ILoggerBuilder`.

_This package does not provide any logging implementations._

#### Configuration in AppSettings.json ####

```json
{
    "ApplicationName": "{AppName}",
    "AppSettings": {
        "Env": "{Environment}"
    },
    "Logging": {
        "MinimumLevel": "Warn",
        "Filters": {
          "Microsoft": "Error",
          "System": "Info"
        },
        "OutputTemplate": "MainTemplateProp",
        "Targets": [
          {
            "Type": "Console",
            "Args": {
              "ImAKey": "ImAValueForAKey"
            },
            "OutputTemplate": "TestingOverrideForTemplate"
          },
          {
            "Type": "RollingFile",
            "Args": {
              "PathFormat": "C:\\Test\\Path\\Magic",
              "FileCount": "15",
              "FileSize": "10000000"
            }
          }
        ]
    }
}
```
*Consult the Provider implementaion for a definitive list of available Target Args for the Target Type.*

##### Filters configuration options #####

`Filters` accepts a Key Value Pair configuration where the Key refers to a CategoryName
of a logger and the Value refers to the minimum logging level.

##### Targets configuration options #####

- `Type`: The type of logging target that is being configured.
  - Console
  - Debug - When providing a compatible log provider can log to VS Output window
  - RollingFile 
  - Service - When logging to Application Insights or Splunk directly
- `Args`: Provider specific Key Value Pair configuration items that will be passed to the `IConfigureLoggerProvider` implementation.
- `OutputTemplate`: When provided will override the main OutputTemplate with a target specific template when supported.


