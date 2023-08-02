﻿namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Exceptions;

internal sealed class CurrencyNotFoundException : ArgumentOutOfRangeException
{
    public CurrencyNotFoundException(string? paramName, object? actualValue, string? message = null)
        : base(paramName, actualValue, message)
    {

    }
}
