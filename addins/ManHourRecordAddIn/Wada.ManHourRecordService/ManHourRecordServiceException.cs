using System.Runtime.Serialization;

namespace Wada.ManHourRecordService;

[Serializable]
public class ManHourRecordServiceException : Exception
{
    public ManHourRecordServiceException()
    {
    }

    public ManHourRecordServiceException(string? message) : base(message)
    {
    }

    public ManHourRecordServiceException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected ManHourRecordServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}