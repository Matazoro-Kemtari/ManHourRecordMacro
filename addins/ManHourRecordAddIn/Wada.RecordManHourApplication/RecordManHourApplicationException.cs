using System.Runtime.Serialization;

namespace Wada.RecordManHourApplication;

[Serializable]
public class RecordManHourApplicationException : Exception
{
    public RecordManHourApplicationException()
    {
    }

    public RecordManHourApplicationException(string? message) : base(message)
    {
    }

    public RecordManHourApplicationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected RecordManHourApplicationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}