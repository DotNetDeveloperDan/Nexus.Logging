using NUnit.Framework;
using System;

namespace Nexus.Logging.Correlator.Contract.Tests
{
    public class DummyTestToMakeBuildHappyWhenOnlyContract
    {
        [Test]
        public void TestStuff()
        {
            var test = new FakeContext();
            Assert.That(test, Is.Not.Null);
        }

    }

    public class FakeContext : ICorrelationContext
    {
        public string CorrelationId { get; } = Guid.NewGuid().ToString();
        public int Sequence { get; }
        public string ParentCorrelationId { get; }
        public string StackId { get; }
        public void IncrementSequence()
        {

        }
    }
}