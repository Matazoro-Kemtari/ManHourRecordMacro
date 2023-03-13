using System.Runtime.Serialization;

namespace Wada.ManHourRecordService
{
    public class RecordCancelServiceException : Exception
    {
        public RecordCancelServiceException()
        {
        }

        public RecordCancelServiceException(string message) : base(message)
        {
        }

        public RecordCancelServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RecordCancelServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
