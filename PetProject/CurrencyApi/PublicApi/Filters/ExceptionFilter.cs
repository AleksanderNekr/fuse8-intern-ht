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

        switch (exception)
        {
            case InvalidFavoriteNameException:
                _logger.LogError("Invalid favorite name");
                context.Result = new NotFoundResult();

                break;
            case OperationCanceledException:
                _logger.LogInformation("Operation was canceled");
                context.Result = new NoContentResult();

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
