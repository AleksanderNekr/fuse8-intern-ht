using Fuse8_ByteMinds.SummerSchool.InternalApi.Exceptions;
using Microsoft.AspNetCore.Mvc;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Middlewares;

public class ExceptionHandlerMiddleware
{
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;
    private readonly RequestDelegate                     _next;

    public ExceptionHandlerMiddleware(ILogger<ExceptionHandlerMiddleware> logger, RequestDelegate next)
    {
        _logger = logger;
        _next   = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiRequestLimitException e)
        {
            const string message = "API request limit exceeded";
            _logger.LogError(e, message);
            context.Response.StatusCode = 429;
            await context.Response.WriteAsJsonAsync(new ObjectResult(message)
                                                    {
                                                        StatusCode = context.Response.StatusCode,
                                                    });
        }
        catch (CurrencyNotFoundException)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new NotFoundResult());
        }
        catch (IncorrectDateException e)
        {
            _logger.LogError(e, "Unsupported date by API");
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new ObjectResult(e.Message)
                                                    {
                                                        StatusCode = context.Response.StatusCode,
                                                    });
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Task was cancelled");
            context.Response.StatusCode = 204;
        }
        catch (Exception e)
        {
            const string message = "Unknown exception";
            _logger.LogError(e, message);
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new ObjectResult(message)
                                                    {
                                                        StatusCode = context.Response.StatusCode
                                                    });
        }
    }
}
