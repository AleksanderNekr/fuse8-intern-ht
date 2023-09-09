namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Exceptions;

internal sealed class IncorrectDateException : Exception
{
    public IncorrectDateException(DateOnly date, DateOnly minDate, DateOnly maxDate)
        : base($"Date should be not earlier than {minDate} and not later than {maxDate}. Was: {date}")
    {
    }
}
