namespace DigTx.Designer.DesignerAssistant.FunctionApp.Exceptions;

using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using DigTx.Designer.FunctionApp.Exceptions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

public class GlobalExceptionHandlerMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Handled InvalidOperationException.");
            await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Handled UnauthorizedAccessException.");
            await WriteErrorResponseAsync(context, HttpStatusCode.Unauthorized, ex.Message);
        }
        catch (ForbiddenAccessException ex)
        {
            _logger.LogError(ex, "Handled ForbiddenAccessException.");
            await WriteErrorResponseAsync(context, HttpStatusCode.Forbidden, ex.Message);
        }
        catch (InvalidRequestException ex)
        {
            _logger.LogError(ex, "Handled InvalidRequestException.");
            await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, "Handled NotFoundException.");
            await WriteErrorResponseAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (NotSupportedException ex)
        {
            _logger.LogError(ex, "Handled NotSupportedException.");
            await WriteErrorResponseAsync(context, HttpStatusCode.InternalServerError, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception.");
            await WriteErrorResponseAsync(context, HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    private static async Task WriteErrorResponseAsync(FunctionContext context, HttpStatusCode statusCode, string message)
    {
        var req = await context.GetHttpRequestDataAsync();
        if (req == null)
        {
            return;
        }

        var response = req.CreateResponse(statusCode);
        var errorPayload = JsonSerializer.Serialize(new { error = message });
        await response.WriteStringAsync(errorPayload);

        context.GetInvocationResult().Value = response;
    }
}
