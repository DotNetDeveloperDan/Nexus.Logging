using System.Collections.Generic;

namespace Nexus.Logging.Harness.IntegrationTests
{
    /// <summary>
    /// Intermediary to parse log lines to determine if structure/placement in relation to json root for properties is correct.
    /// </summary>
    public class LogEntry
    {
        public string ApplicationName { get; set; }
        public string Environment { get; set; }
        public string Type { get; set; }
        public string InstanceId { get; set; }
        public string UserName { get; set; }
        public string StackId { get; set; }
        public string ParentCorrelationId { get; set; }
        public string CorrelationId { get; set; }
        public string Sequence { get; set; }
        public string MachineName { get; set; }
        public string TimeStamp { get; set; }
        public string LogLevel { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public string TargetSite { get; set; }
        public string InnerExMessage { get; set; }
        public string InnerExStackTrace { get; set; }
        public string StackTrace { get; set; }
        public Dictionary<string, object> LogDetails { get; set; }
    }
}
