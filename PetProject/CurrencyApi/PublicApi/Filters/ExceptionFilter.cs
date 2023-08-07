using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Filters;

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

        _logger.LogError(exception, "Unknown exception");
        context.Result = new ObjectResult(exception.Message)
                         {
                             StatusCode = 500,
                         };

        context.ExceptionHandled = true;
    }
}
