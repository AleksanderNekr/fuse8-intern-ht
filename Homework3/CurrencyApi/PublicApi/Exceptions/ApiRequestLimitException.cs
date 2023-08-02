namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Exceptions;

internal sealed class ApiRequestLimitException : Exception
{
    public ApiRequestLimitException(string? message = null) : base(message)
    {

    }
}
