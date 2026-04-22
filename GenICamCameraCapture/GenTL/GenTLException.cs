namespace GenICamCameraCapture.GenTL;

public class GenTLException : Exception
{
    public GC_ERROR ErrorCode { get; }

    public GenTLException(GC_ERROR errorCode)
        : base($"GenTL Error: {errorCode} ({(int)errorCode})")
    {
        ErrorCode = errorCode;
    }

    public GenTLException(GC_ERROR errorCode, string message)
        : base($"GenTL Error: {errorCode} ({(int)errorCode}) - {message}")
    {
        ErrorCode = errorCode;
    }
}
