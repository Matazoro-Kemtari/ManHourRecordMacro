using System.Runtime.Serialization;

namespace Wada.RecordManHourApplication
{
    [Serializable]
    public class ManHourRecordExistsException : Exception
    {
        public ManHourRecordExistsException()
        {
        }

        public ManHourRecordExistsException(string? message) : base(message)
        {
        }

        public ManHourRecordExistsException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ManHourRecordExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}