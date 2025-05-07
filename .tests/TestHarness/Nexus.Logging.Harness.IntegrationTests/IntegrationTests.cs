using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Nexus.Logging.Correlator;
using NUnit.Framework;

namespace Nexus.Logging.Harness.IntegrationTests
{
    [TestFixture]
    public class IntegrationTests
    {
        private Dictionary<string, IEnumerable<string>> _logs = new();
        private readonly string _logSuffix = $"{DateTime.Now:yyyyMMdd}.log";
        private IEnumerable<string> _service1Logs;
        private IEnumerable<string> _service2Logs;
        private IEnumerable<string> _service3Logs;
        private IEnumerable<string> _serviceNoCorrelatorLogs;
        private List<CorrelationContext> _service1CorrelationContexts;
        private List<CorrelationContext> _service2CorrelationContexts;
        private List<CorrelationContext> _service3CorrelationContexts;
        private List<CorrelationContext> _serviceNoCorrelatorCorrelationContexts;
        public static HttpClient Client { get; set; }

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            DeleteLogs();

            Client = Initializer.InitializeChain();

            // Create new logs
            await Client.GetStringAsync("/firstendpoint");

            // Retrieve logs
            _logs = GetAllServiceLogs();
            _service1Logs = _logs[$"NexusLoggingHarnessService1-{_logSuffix}"];
            _service2Logs = _logs[$"NexusLoggingHarnessService2-{_logSuffix}"];
            _service3Logs = _logs[$"NexusLoggingHarnessService3-{_logSuffix}"];
            _serviceNoCorrelatorLogs = _logs[$"NexusLoggingHarnessServiceNoCorrelator-{_logSuffix}"];

            _service1CorrelationContexts = _service1Logs.Select(CreateCorrelationContext).ToList();
            _service2CorrelationContexts = _service2Logs.Select(CreateCorrelationContext).ToList();
            _service3CorrelationContexts = _service3Logs.Select(CreateCorrelationContext).ToList();
            _serviceNoCorrelatorCorrelationContexts = _serviceNoCorrelatorLogs.Select(CreateCorrelationContext).ToList();
        }

        [Test]
        public void Logs_Are_Generated_For_Each_Service()
        {
            Assert.That(_logs.Count, Is.EqualTo(4));
        }

        #region CorrelationTests

        [Test]
        public void ParentCorrelationId_Is_CorrelationId_Of_Previous_Service()
        {
            var svc2Parent = _service2CorrelationContexts[0].ParentCorrelationId;
            var svc1Id = _service1CorrelationContexts[0].CorrelationId;
            Assert.That(svc2Parent, Is.EqualTo(svc1Id));

            var svc3Parent = _service3CorrelationContexts[0].ParentCorrelationId;
            var svc2Id = _service2CorrelationContexts[0].CorrelationId;
            Assert.That(svc3Parent, Is.EqualTo(svc2Id));
        }

        [Test]
        public void There_Are_No_Correlation_Details_In_Service_No_Correlator()
        {
            var ctx = _serviceNoCorrelatorCorrelationContexts[0];
            Assert.That(ctx.ParentCorrelationId, Is.Null.Or.Empty);
            Assert.That(ctx.CorrelationId, Is.Null.Or.Empty);
            Assert.That(ctx.Sequence, Is.EqualTo(0));
        }

        [Test]
        public void Do_Not_Set_ParentCorrelationId_If_No_CorrelationId_Was_Sent()
        {
            var svc3Parent = _service3CorrelationContexts[0].ParentCorrelationId;
            var noCorrId = _serviceNoCorrelatorCorrelationContexts[0].CorrelationId;
            Assert.That(svc3Parent, Is.Not.EqualTo(noCorrId));
        }

        [Test]
        public void New_CorrelationId_When_No_CorrelationId_Is_In_Header()
        {
            var ctx = _service3CorrelationContexts[2];
            Assert.That(ctx.CorrelationId, Is.Not.Null.And.Not.Empty);
            Assert.That(ctx.ParentCorrelationId, Is.Null.Or.Empty);
        }

        [Test]
        public void RequestId_Should_PersistSameValueAcrossServiceBoundaries()
        {
            var req2 = _service2CorrelationContexts[0].StackId;
            var req1 = _service1CorrelationContexts[0].StackId;
            Assert.That(req2, Is.EqualTo(req1));

            var req3 = _service3CorrelationContexts[0].StackId;
            Assert.That(req3, Is.EqualTo(req2));

            var noReq = _serviceNoCorrelatorCorrelationContexts[0].StackId;
            Assert.That(noReq, Is.Empty);
        }

        #endregion

        #region Private Methods

        private void DeleteLogs()
        {
            foreach (var file in Directory.EnumerateFiles(
                         Utilities.GetSolutionDirectory(),
                         "*-????????.log",
                         SearchOption.AllDirectories))
            {
                File.Delete(file);
            }
        }

        private Dictionary<string, IEnumerable<string>> GetAllServiceLogs()
        {
            return Directory.EnumerateFiles(Utilities.GetSolutionDirectory(), $"*{_logSuffix}", SearchOption.AllDirectories).ToDictionary(filePath => Path.GetFileName(filePath), filePath => ReadLines(filePath));
        }

        private CorrelationContext CreateCorrelationContext(string logEntry)
        {
            var log = JsonSerializer.Deserialize<LogEntry>(logEntry);
            int.TryParse(log.Sequence, out var seq);
            return new CorrelationContext(log.CorrelationId, log.ParentCorrelationId, log.StackId, seq);
        }

        private static IEnumerable<string> ReadLines(string path)
        {
            using var fs = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite,
                0x1000,
                FileOptions.SequentialScan);
            using var sr = new StreamReader(fs, Encoding.UTF8);
            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                yield return line;
            }
        }

        #endregion
    }
}
