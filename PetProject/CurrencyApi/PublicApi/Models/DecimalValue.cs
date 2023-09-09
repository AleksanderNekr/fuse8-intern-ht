// ReSharper disable once CheckNamespace
namespace Fuse8_ByteMinds.SummerSchool.Grpc;

public partial class DecimalValue
{
    private const decimal NanoFactor = 1_000_000_000_000;

    /// <summary>
    /// Представляет реализацию decimal для gRPC сервиса.
    /// </summary>
    /// <param name="units">Целая часть.</param>
    /// <param name="nanos">Дробная часть.</param>
    public DecimalValue(long units, long nanos)
    {
        Units = units;
        Nanos = nanos;
    }

    /// <summary>
    /// Неявная конвертация из данного типа в тип decimal CLR.
    /// </summary>
    /// <param name="grpcDecimalValue">Число типа <see cref="DecimalValue"/>.</param>
    /// <returns>Число типа <see cref="decimal"/>.</returns>
    public static implicit operator decimal(DecimalValue grpcDecimalValue)
    {
        return grpcDecimalValue.Units + grpcDecimalValue.Nanos / NanoFactor;
    }

    /// <summary>
    /// Неявная конвертация из типа decimal CLR в тип <see cref="DecimalValue"/>.
    /// </summary>
    /// <param name="decimalValue">Число типа decimal CLR.</param>
    /// <returns>Число типа <see cref="DecimalValue"/>.</returns>
    public static implicit operator DecimalValue(decimal decimalValue)
    {
        var     units      = decimal.ToInt64(decimalValue);
        decimal afterPoint = decimalValue - units;
        var     nanos      = decimal.ToInt64(afterPoint * NanoFactor);

        return new DecimalValue(units, nanos);
    }
}
