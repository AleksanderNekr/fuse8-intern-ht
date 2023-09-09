namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Exceptions;

internal sealed class ApiRequestLimitException : Exception
{
    public ApiRequestLimitException(string? message = null) : base(message)
    {

    }
}
