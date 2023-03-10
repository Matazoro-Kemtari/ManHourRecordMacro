using System.Runtime.Serialization;

namespace Wada.Wada.ManHourRecordService.WorkingLedgerAggregation
{
    [Serializable]
    public class WorkingLedgerAggregationException : Exception
    {
        public WorkingLedgerAggregationException()
        {
        }

        public WorkingLedgerAggregationException(string? message) : base(message)
        {
        }

        public WorkingLedgerAggregationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected WorkingLedgerAggregationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}