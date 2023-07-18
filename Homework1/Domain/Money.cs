namespace Fuse8_ByteMinds.SummerSchool.Domain;

/// <summary>
/// Модель для хранения денег
/// </summary>
public sealed class Money : IComparable<Money>
{
    public Money(int rubles, int kopecks)
        : this(false, rubles, kopecks)
    {
    }

    public Money(bool isNegative, int rubles, int kopecks)
    {
        if (rubles < 0)
        {
            throw new ArgumentException("Количество рублей не может быть отрицательным");
        }

        if (kopecks is < 0 or > 99)
        {
            throw new ArgumentException("Количество копеек должно быть от 0 до 99 включительно");
        }

        if (isNegative && rubles == 0 && kopecks == 0)
        {
            throw new ArgumentException("Нулевой баланс не может быть отрицательным");
        }

        this.IsNegative = isNegative;
        this.Rubles     = rubles;
        this.Kopeks     = kopecks;
    }

    /// <summary>
    /// Отрицательное значение
    /// </summary>
    public bool IsNegative { get; }

    /// <summary>
    /// Число рублей
    /// </summary>
    public int Rubles { get; }

    /// <summary>
    /// Количество копеек
    /// </summary>
    public int Kopeks { get; }

    private const int KopeсksInRuble = 100;

    public int CompareTo(Money? other)
    {
        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        if (other is null)
        {
            return 1;
        }

        return this.ConvertToKopeсks().CompareTo(other.ConvertToKopeсks());
    }

    public static bool operator <(Money? left, Money? right)
    {
        return left?.CompareTo(right) < 0;
    }

    public static bool operator >(Money? left, Money? right)
    {
        return left?.CompareTo(right) > 0;
    }

    public static bool operator <=(Money? left, Money? right)
    {
        return left?.CompareTo(right) <= 0;
    }

    public static bool operator >=(Money? left, Money? right)
    {
        return left?.CompareTo(right) >= 0;
    }

    public static Money operator +(in Money? left, in Money? right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        int kopeсksSum = left.ConvertToKopeсks() + right.ConvertToKopeсks();

        return ConvertToMoney(kopeсksSum);
    }

    public static Money operator -(in Money? left, in Money? right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        int kopeсksDif = left.ConvertToKopeсks() - right.ConvertToKopeсks();

        return ConvertToMoney(kopeсksDif);
    }

    private int ConvertToKopeсks()
    {
        int kopecks = this.Rubles * KopeсksInRuble + this.Kopeks;
        if (this.IsNegative)
        {
            return -1 * kopecks;
        }

        return kopecks;
    }

    private static Money ConvertToMoney(in int kopeсks)
    {
        bool isNegative = kopeсks < 0;
        int unsignedKopeсks = isNegative
                                  ? -1 * kopeсks
                                  : kopeсks;

        return new Money(isNegative, unsignedKopeсks / KopeсksInRuble, unsignedKopeсks % KopeсksInRuble);
    }

    public override bool Equals(object? obj)
    {
        return obj is Money other && this.CompareTo(other) == 0;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.IsNegative, this.Rubles, this.Kopeks);
    }

    public override string ToString()
    {
        return $"{(this.IsNegative
                       ? "-"
                       : string.Empty)}{this.Rubles}.{this.Kopeks}";
    }
}
