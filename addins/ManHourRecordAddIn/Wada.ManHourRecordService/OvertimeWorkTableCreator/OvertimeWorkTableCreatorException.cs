using System.Runtime.Serialization;

namespace Wada.ManHourRecordService.OvertimeWorkTableCreator
{
    [Serializable]
    public class OvertimeWorkTableCreatorException : Exception
    {
        public OvertimeWorkTableCreatorException()
        {
        }

        public OvertimeWorkTableCreatorException(string? message) : base(message)
        {
        }

        public OvertimeWorkTableCreatorException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected OvertimeWorkTableCreatorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}