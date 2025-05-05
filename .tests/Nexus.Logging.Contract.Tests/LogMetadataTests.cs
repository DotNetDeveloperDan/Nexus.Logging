using NUnit.Framework;

namespace Nexus.Logging.Contract.Tests
{
    public class LogMetadataTests
    {
        [Test]
        public void When_SettingPropertyValue_Should_AddToUnderlyingDictionary()
        {
            const string TestVal = "TestingLeaseId";
            var ld = new TestLogDetails
            {
                LeaseId = TestVal
            };

            Assert.That(ld.Count, Is.EqualTo(1));
            Assert.That(ld.LeaseId, Is.EqualTo(TestVal));
        }

        [Test]
        public void When_SettingValueWithIndexer_Should_AddToUnderlyingDictionary()
        {
            const string TestVal = "TestingLeaseId";
            var ld = new TestLogDetails
            {
                ["LeaseId"] = TestVal
            };

            Assert.That(ld.Count, Is.EqualTo(1));
            Assert.That(ld["LeaseId"], Is.EqualTo(TestVal));
        }

        [Test]
        public void When_GettingPropertyThatWasNotPreviouslySet_Should_ReturnNull()
        {
            var ld = new TestLogDetails();

            Assert.That(ld.Count, Is.EqualTo(0));
            Assert.That(ld.LeaseId, Is.Null);
        }

        [Test]
        public void When_GettingPropertyFromIndexerThatWasNotPreviouslySet_Should_ReturnNull()
        {
            var ld = new TestLogDetails();

            Assert.That(ld.Count, Is.EqualTo(0));
            Assert.That(ld["LeaseId"], Is.Null);
        }

        [Test]
        public void When_SettingValueWithInitializer_Should_AddToUnderlyingDictionary()
        {
            const string TestVal = "TestingLeaseId";
            var ld = new TestLogDetails() { { "LeaseId", TestVal } };

            Assert.That(ld.Count, Is.EqualTo(1));
            Assert.That(ld["LeaseId"], Is.EqualTo(TestVal));
        }
    }

    public class TestLogDetails : LogMetadataBase
    {
        public string LeaseId
        {
            get => GetValue();
            set => SetValue(value);
        }

        public string StoreId
        {
            get => GetValue();
            set => SetValue(value);
        }
    }
}
