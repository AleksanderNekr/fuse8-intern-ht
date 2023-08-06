using Fuse8_ByteMinds.SummerSchool.InternalApi.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Filters;

internal class ExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ExceptionFilter> _logger;

    public ExceptionFilter(ILogger<ExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        Exception exception = context.Exception;

        switch (exception)
        {
            case ApiRequestLimitException:
                _logger.LogError(exception, "API request limit exceeded");
                context.Result = new ObjectResult(exception.Message)
                                 {
                                     StatusCode = 429,
                                 };

                break;
            case CurrencyNotFoundException:
                context.Result = new NotFoundResult();

                break;
            case IncorrectDateException:
                _logger.LogError(exception, "Unsupported date by API");
                context.Result = new BadRequestResult();

                break;
            default:
                _logger.LogError(exception, "Unknown exception");
                context.Result = new ObjectResult(exception.Message)
                                 {
                                     StatusCode = 500,
                                 };

                break;
        }

        context.ExceptionHandled = true;
    }
}
