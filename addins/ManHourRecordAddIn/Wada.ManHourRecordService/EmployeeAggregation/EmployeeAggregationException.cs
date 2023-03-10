using System.Runtime.Serialization;

namespace Wada.ManHourRecordService.EmployeeAggregation
{
    [Serializable]
    public class EmployeeAggregationException : Exception
    {
        public EmployeeAggregationException()
        {
        }

        public EmployeeAggregationException(string? message) : base(message)
        {
        }

        public EmployeeAggregationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected EmployeeAggregationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}