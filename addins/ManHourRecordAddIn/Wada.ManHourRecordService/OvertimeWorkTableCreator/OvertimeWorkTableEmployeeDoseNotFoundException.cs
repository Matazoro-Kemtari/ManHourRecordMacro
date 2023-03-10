using System.Runtime.Serialization;

namespace Wada.ManHourRecordService.OvertimeWorkTableCreator
{
    public class OvertimeWorkTableEmployeeDoseNotFoundException : Exception
    {
        public OvertimeWorkTableEmployeeDoseNotFoundException()
        {
        }

        public OvertimeWorkTableEmployeeDoseNotFoundException(string? message) : base(message)
        {
        }

        public OvertimeWorkTableEmployeeDoseNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected OvertimeWorkTableEmployeeDoseNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
