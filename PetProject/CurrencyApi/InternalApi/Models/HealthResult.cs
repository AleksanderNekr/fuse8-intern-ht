namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Models;

/// <summary>
///     Результат проверки работоспособности API
/// </summary>
public sealed record HealthResult
{
    /// <summary>
    ///     Статус API
    /// </summary>
    public enum CheckStatus
    {
        /// <summary>
        ///     API работает
        /// </summary>
        Ok = 1,

        /// <summary>
        ///     Ошибка в работе API
        /// </summary>
        Failed = 2,
    }

    /// <summary>
    ///     Дата проверки
    /// </summary>
    public DateTimeOffset CheckedOn { get; init; }

    /// <summary>
    ///     Статус работоспособности API
    /// </summary>
    public CheckStatus Status { get; init; }
}
