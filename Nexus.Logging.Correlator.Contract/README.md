# Nexus.Logging.Correlator.Contract #

Contract for data that is passed on request headers between services and systems.

_This package does not provide any logging implementations._

## Usage ##

`ICorrelationContextAccessor` should be used by a consumer to access the 
underlying `ICorrelationContext` in a thread safe manner.

