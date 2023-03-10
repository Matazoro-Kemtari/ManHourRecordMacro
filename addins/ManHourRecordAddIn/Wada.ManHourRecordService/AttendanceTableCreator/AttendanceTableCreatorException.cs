using System.Runtime.Serialization;

namespace Wada.ManHourRecordService.AttendanceTableCreator
{
    [Serializable]
    public class AttendanceTableCreatorException : Exception
    {
        public AttendanceTableCreatorException()
        {
        }

        public AttendanceTableCreatorException(string? message) : base(message)
        {
        }

        public AttendanceTableCreatorException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected AttendanceTableCreatorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}