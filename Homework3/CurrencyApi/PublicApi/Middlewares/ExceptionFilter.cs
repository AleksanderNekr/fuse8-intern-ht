using Fuse8_ByteMinds.SummerSchool.PublicApi.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Middlewares;

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
                _logger.LogError("API request limit exceeded\n{Message}", exception.Message);
                context.Result = new ObjectResult(exception.Message)
                                 {
                                     StatusCode = 429
                                 };

                break;
            case CurrencyNotFoundException:
                context.Result = new NotFoundResult();

                break;
            default:
                _logger.LogError("Exception: {Message}", exception.Message);
                context.Result = new ObjectResult(exception.Message)
                                 {
                                     StatusCode = 500
                                 };

                break;
        }

        context.ExceptionHandled = true;
    }
}
