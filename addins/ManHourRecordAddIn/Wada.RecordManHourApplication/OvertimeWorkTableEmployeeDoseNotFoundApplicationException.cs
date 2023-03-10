using System.Runtime.Serialization;

namespace Wada.RecordManHourApplication
{
    [Serializable]
    public class OvertimeWorkTableEmployeeDoseNotFoundApplicationException : Exception
    {
        public OvertimeWorkTableEmployeeDoseNotFoundApplicationException()
        {
        }

        public OvertimeWorkTableEmployeeDoseNotFoundApplicationException(string message) : base(message)
        {
        }

        public OvertimeWorkTableEmployeeDoseNotFoundApplicationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OvertimeWorkTableEmployeeDoseNotFoundApplicationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}