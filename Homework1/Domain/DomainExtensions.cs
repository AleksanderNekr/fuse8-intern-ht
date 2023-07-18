namespace Fuse8_ByteMinds.SummerSchool.Domain;

public static class DomainExtensions
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? enumerable)
    {
        return enumerable is null || !enumerable.Any();
    }

    public static string JoinToString<T>(this IEnumerable<T> enumerable, string separator)
    {
        return string.Join(separator, enumerable);
    }

    public static int DaysCountBetween(this DateTimeOffset dateTimeOffset1, DateTimeOffset dateTimeOffset2)
    {
        return Math.Abs((dateTimeOffset2 - dateTimeOffset1).Days);
    }
}
