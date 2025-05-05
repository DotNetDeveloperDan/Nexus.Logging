using System;
using System.Collections.Generic;
using Serilog.Events;
using Serilog.Parsing;

namespace Nexus.Logging.Serilog.Tests;

/// <summary>
///     Fake data for testing logging events.
/// </summary>
public static class LogEventData
{
    /// <summary>
    ///     Embedded characters that should be escaped.
    /// </summary>
    /// <returns></returns>
    public static LogEvent UnSanitizedCharacters()
    {
        return new LogEvent(DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            new MessageTemplateParser().Parse(
                "  Test log message with embedded \t and 'single quotes' and \"quotes\" and carriage return\r\n"),
            new List<LogEventProperty>
            {
                new("Scope",
                    new ScalarValue(new KeyValuePair<string, string>(LogProperty.ApplicationName, "TestingAppName")))
            });
    }

    /// <summary>
    ///     An Info event with a single scope.
    /// </summary>
    /// <returns></returns>
    public static LogEvent InfoEvent()
    {
        return new LogEvent(DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            new MessageTemplateParser().Parse("Test log message"),
            new List<LogEventProperty>
            {
                new("Scope",
                    new ScalarValue(new KeyValuePair<string, string>(LogProperty.ApplicationName, "TestingAppName")))
            });
    }

    /// <summary>
    ///     An Info event with multiple scopes.
    /// </summary>
    /// <returns></returns>
    public static LogEvent InfoEventWithNestedScopes()
    {
        return new LogEvent(DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            new MessageTemplateParser().Parse("Test log message"),
            new List<LogEventProperty>
            {
                new("Scope", new SequenceValue(
                    new List<LogEventPropertyValue>
                    {
                        new ScalarValue(new KeyValuePair<string, string>(LogProperty.ApplicationName,
                            "TestingAppName")),
                        new ScalarValue(new KeyValuePair<string, string>(LogProperty.Sequence,
                            "VerifyThatSequenceIsHere")),
                        new ScalarValue(new KeyValuePair<string, string>("NonStandard", "ShouldBeInLogDetails")),
                        new ScalarValue(new KeyValuePair<string, string>("Jsonified",
                            "{\"JsonProp\":\"JsonVal\",\"AnotherJsonProp\":\"AnotherJsonVal\"}"))
                    })
                )
            });
    }

    public static LogEvent EventWithAllProperties()
    {
        InvalidOperationException ex = null;
        try
        {
            InvokeNestedException();
        }
        catch (InvalidOperationException ioe)
        {
            ex = ioe;
        }

        return new LogEvent(DateTimeOffset.UtcNow,
            LogEventLevel.Error,
            ex,
            new MessageTemplateParser().Parse("Fully populated log message to verify all fields are present"),
            new List<LogEventProperty>
            {
                new("Scope", new SequenceValue(
                    new List<LogEventPropertyValue>
                    {
                        new ScalarValue(new KeyValuePair<string, string>(LogProperty.ApplicationName,
                            "Testing" + nameof(LogProperty.ApplicationName))),
                        new ScalarValue(new KeyValuePair<string, string>(LogProperty.Environment,
                            "Testing" + nameof(LogProperty.Environment))),
                        new ScalarValue(new KeyValuePair<string, string>(LogProperty.InstanceId,
                            "Testing" + nameof(LogProperty.InstanceId))),
                        new ScalarValue(new KeyValuePair<string, string>(LogProperty.MachineName,
                            "Testing" + nameof(LogProperty.MachineName))),
                        new ScalarValue(new KeyValuePair<string, string>(LogProperty.UserName,
                            "Testing" + nameof(LogProperty.UserName))),
                        new ScalarValue(new KeyValuePair<string, string>(LogProperty.CorrelationId,
                            "Testing" + nameof(LogProperty.CorrelationId))),
                        new ScalarValue(new KeyValuePair<string, string>(LogProperty.ParentCorrelationId,
                            "Testing" + nameof(LogProperty.ParentCorrelationId))),
                        new ScalarValue(new KeyValuePair<string, string>(LogProperty.Sequence,
                            "Testing" + nameof(LogProperty.Sequence))),
                        new ScalarValue(new KeyValuePair<string, string>(LogProperty.StackId,
                            "Testing" + nameof(LogProperty.StackId))),
                        new ScalarValue(new KeyValuePair<string, string>("NonGlobal1", "ShowNonGlobal1InLogDetails")),
                        new ScalarValue(new KeyValuePair<string, string>("NonGlobal2", "ShowNonGlobal2InLogDetails"))
                    })
                )
            });
    }

    /// <summary>
    ///     An Error event with a single exception.
    /// </summary>
    /// <returns></returns>
    public static LogEvent SingleErrorEvent()
    {
        InvalidOperationException ex = null;
        try
        {
            InvokeSingleExceptionStackTrace();
        }
        catch (InvalidOperationException ioe)
        {
            ex = ioe;
        }

        return new LogEvent(DateTimeOffset.UtcNow,
            LogEventLevel.Error,
            ex,
            new MessageTemplateParser().Parse("Test log message"),
            new List<LogEventProperty>
            {
                new("Scope",
                    new ScalarValue(new KeyValuePair<string, string>(LogProperty.ApplicationName, "TestingAppName")))
            });
    }

    /// <summary>
    ///     An Error event with an inner exception.
    /// </summary>
    /// <returns></returns>
    public static LogEvent InnerErrorEvent()
    {
        InvalidOperationException ex = null;
        try
        {
            InvokeNestedException();
        }
        catch (InvalidOperationException ioe)
        {
            ex = ioe;
        }

        return new LogEvent(DateTimeOffset.UtcNow,
            LogEventLevel.Error,
            ex,
            new MessageTemplateParser().Parse("Test log message"),
            new List<LogEventProperty>
            {
                new("Scope",
                    new ScalarValue(new KeyValuePair<string, string>(LogProperty.ApplicationName, "TestingAppName")))
            });
    }

    /// <summary>
    ///     An Info event with multiple scopes at the root level.
    /// </summary>
    /// <returns></returns>
    public static LogEvent InfoEventWithRootLevelAppScopes()
    {
        return new LogEvent(DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            new MessageTemplateParser().Parse("Test log message"),
            new List<LogEventProperty>
            {
                new(LogProperty.ApplicationName, new ScalarValue("TestingApplicationName")),
                new(LogProperty.Sequence, new ScalarValue("TestingSequence")),
                new("NonStandard", new ScalarValue("ShouldBeInLogDetails")),
                new("Jsonified", new ScalarValue("{\"JsonProp\":\"JsonVal\",\"AnotherJsonProp\":\"AnotherJsonVal\"}")),
                new("IntValue", new ScalarValue(1111222))
            });
    }


    /// <summary>
    ///     An Info event with multiple scopes at the root level includes an Anonymous Type.
    /// </summary>
    /// <returns></returns>
    public static LogEvent InfoEventWithAnonTypesAsScopes()
    {
        return new LogEvent(DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            new MessageTemplateParser().Parse("Test log message"),
            new List<LogEventProperty>
            {
                new(LogProperty.ApplicationName, new ScalarValue("TestingApplicationName")),
                new(LogProperty.Sequence, new ScalarValue("TestingSequence")),
                new("NonStandard", new ScalarValue("ShouldBeInLogDetails")),
                new("Jsonified", new ScalarValue("{\"JsonProp\":\"JsonVal\",\"AnotherJsonProp\":\"AnotherJsonVal\"}")),
                new("StringifiedAnon",
                    new ScalarValue(new { Prop1 = "IAmAnonymous", Prop2 = "MoreStuffToSee", Prop3 = true }))
            });
        ;
    }

    // These are used to simulate stack traces
    public static void InvokeSingleExceptionStackTrace()
    {
        SingleException();
    }

    public static void SingleException()
    {
        throw new InvalidOperationException("Bad things happened");
    }

    public static void InvokeNestedException()
    {
        NestedException();
    }

    public static void NestedException()
    {
        throw new InvalidOperationException("Bad things happened", new ArgumentNullException("Something is null"));
    }
}