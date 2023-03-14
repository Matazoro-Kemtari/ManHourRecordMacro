using System.Runtime.Serialization;

namespace Wada.ManHourRecordService
{
    public class RecordAbortException : DomainException
    {
        public RecordAbortException()
        {
        }

        public RecordAbortException(string message) : base(message)
        {
        }

        public RecordAbortException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RecordAbortException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
