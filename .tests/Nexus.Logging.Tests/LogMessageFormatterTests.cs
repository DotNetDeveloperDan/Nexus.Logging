using System.Collections.Generic;
using NUnit.Framework;

namespace Nexus.Logging.Configuration.Tests
{
    public class LogMessageFormatterTests
    {
        [Test]
        public void When_MessageHasNoParametersAndDataProvided_Should_ReturnOriginalMessage()
        {
            const string message = "This is an uformatted message to test.";
            var formatter = new LogMessageFormatter(message);
            var result = formatter.Format(LogMessageFormatterTestData.FakeData);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(message));
            Assert.That(formatter.TemplateKeys.Length, Is.EqualTo(0));
        }

        [Test]
        public void When_KeysValuesValuesMatch_Should_ReplaceMatches()
        {
            var formatter = new LogMessageFormatter(
                "This is a formatted {Something} to test replacement {anotherSomething}.");
            var result = formatter.Format(LogMessageFormatterTestData.FakeData);

            Assert.That(result, Is.Not.Null);
            Assert.That(
                result,
                Is.EqualTo("This is a formatted message to test replacement values."));
        }

        [Test]
        public void When_KeysValuesWithKeyNameMismatch_Should_ReplaceMatchesAndNullKeyNameMismatch()
        {
            var formatter = new LogMessageFormatter(
                "This is a formatted {Something} to test replacement {AnotherSomething}.");
            var result = formatter.Format(LogMessageFormatterTestData.FakeData);

            Assert.That(result, Is.Not.Null);
            Assert.That(
                result,
                Is.EqualTo("This is a formatted message to test replacement (null)."));
        }

        [Test]
        public void When_HasMatchingKeysValuesAndMissingKeyValue_Should_ReplaceMatchesAndNullUnmatched()
        {
            var formatter = new LogMessageFormatter(
                "This is a formatted {Something} to test replacement {anotherSomething} not matched is {unmatched}.");
            var result = formatter.Format(LogMessageFormatterTestData.FakeData);

            Assert.That(result, Is.Not.Null);
            Assert.That(
                result,
                Is.EqualTo("This is a formatted message to test replacement values not matched is (null)."));
        }

        [Test]
        public void When_HasMatchingKeysAndValueIsNull_Should_ReplaceMatchesAndNullStringifyNullValue()
        {
            var formatter = new LogMessageFormatter(
                "This is a formatted {Something} to test replacement {anotherSomething} null value is {nullify}.");
            var data = new Dictionary<string, object>(LogMessageFormatterTestData.FakeData)
            {
                ["nullify"] = null
            };
            var result = formatter.Format(data);

            Assert.That(result, Is.Not.Null);
            Assert.That(
                result,
                Is.EqualTo("This is a formatted message to test replacement values null value is (null)."));
        }

        [Test]
        public void When_MetadataContainsAnonymousTypeWithSerializeNotation_Should_FormatMetadataAsJson()
        {
            var formatter = new LogMessageFormatter(
                "This is a message with {@AnonValue} that should be serialized as Json");
            var result = formatter.Format(LogMessageFormatterTestData.LiteralAndAnonTypes);

            Assert.That(result, Is.Not.Null);
            Assert.That(
                result,
                Is.EqualTo(
                    "This is a message with {\"Prop1\":\"PropString\",\"Prop2\":332211} that should be serialized as Json"));
        }

        [Test]
        public void When_MetadataContainsObjectNoToString_Should_FormatMetadataAsJson()
        {
            var formatter = new LogMessageFormatter(
                "This is a message with object {FooScope} that should be serialized as Json");
            var result = formatter.Format(LogMessageFormatterTestData.ObjectType);

            Assert.That(result, Is.Not.Null);
            Assert.That(
                result,
                Is.EqualTo(
                    "This is a message with object {\"FooString\":\"FooStringValue\",\"FooBool\":true,\"FooInt\":442266} that should be serialized as Json"));
        }

        [Test]
        public void When_MetadataContainsObjectWithToString_Should_FormatMetadataAsObjectToString()
        {
            var formatter = new LogMessageFormatter(
                "This is a message with {FooScope} that should be serialized using ToString");
            var result = formatter.Format(LogMessageFormatterTestData.ObjectTypeWithToStringOverride);

            Assert.That(result, Is.Not.Null);
            Assert.That(
                result,
                Is.EqualTo(
                    "This is a message with FooString=FooStringValue;FooBool=True;FooInt=442266; that should be serialized using ToString"));
        }
    }

    public static class LogMessageFormatterTestData
    {
        public static IDictionary<string, object> FakeData =>
            new Dictionary<string, object>
            {
                ["Something"] = "message",
                ["anotherSomething"] = "values"
            };

        public static IDictionary<string, object> LiteralAndAnonTypes =>
            new Dictionary<string, object>
            {
                ["StringValue"] = "ReplacedString",
                ["IntValue"] = 112233,
                ["@AnonValue"] = new { Prop1 = "PropString", Prop2 = 332211 }
            };

        public static IDictionary<string, object> ObjectType =>
            new Dictionary<string, object>
            {
                ["FooScope"] = new FooScope()
            };

        public static IDictionary<string, object> ObjectTypeWithToStringOverride =>
            new Dictionary<string, object>
            {
                ["FooScope"] = new FooScopeWithToString()
            };

        public class FooScope
        {
            public string FooString { get; set; } = "FooStringValue";
            public bool FooBool { get; set; } = true;
            public int FooInt { get; set; } = 442266;
        }

        public class FooScopeWithToString
        {
            public string FooString { get; set; } = "FooStringValue";
            public bool FooBool { get; set; } = true;
            public int FooInt { get; set; } = 442266;

            public override string ToString() =>
                $"{nameof(FooString)}={FooString};{nameof(FooBool)}={FooBool};{nameof(FooInt)}={FooInt};";
        }
    }
}
