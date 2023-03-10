using System.Runtime.Serialization;

namespace Wada.ManHourRecordService.ValueObjects
{
    [Serializable]
    public class WorkingNumberException : Exception
    {
        public WorkingNumberException()
        {
        }

        public WorkingNumberException(string? message) : base(message)
        {
        }

        public WorkingNumberException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected WorkingNumberException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}