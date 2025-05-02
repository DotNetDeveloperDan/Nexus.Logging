using Nexus.Logging.Correlator.Contract;

namespace Nexus.Logging.Correlator.Extensions
{
    public static class CorrelationContextExtensions
    {
        /// <summary>
        /// Converts CorrelationContext object to dictionary
        /// </summary>
        /// <param name="correlationContext"></param>
        /// <returns>Dictionary version of CorrelationContext object</returns>
        public static Dictionary<string, object> ToDictionary(this ICorrelationContext correlationContext)
        {
            var correlationDict = new Dictionary<string, object>
            {
                { nameof(correlationContext.CorrelationId), correlationContext.CorrelationId },
                { nameof(correlationContext.Sequence), correlationContext.Sequence.ToString() },
                { nameof(correlationContext.ParentCorrelationId), correlationContext.ParentCorrelationId },
                { nameof(correlationContext.StackId), correlationContext.StackId }
            };

            return correlationDict;
        }
    }
}