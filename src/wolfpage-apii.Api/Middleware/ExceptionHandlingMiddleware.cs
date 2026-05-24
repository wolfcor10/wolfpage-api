using System.Net;
using System.Text.Json;
using FluentValidation;

namespace WolfPage.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Error de validación.");
            await WriteProblem(context, HttpStatusCode.BadRequest, "Validation failed",
                ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}").ToArray());
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operación inválida.");
            await WriteProblem(context, HttpStatusCode.BadRequest, "Invalid operation", new[] { ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Acceso no autorizado.");
            await WriteProblem(context, HttpStatusCode.Unauthorized, "Unauthorized", new[] { ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado.");
            await WriteProblem(context, HttpStatusCode.InternalServerError, "Internal Server Error",
                new[] { "Ocurrió un error inesperado." });
        }
    }

    private static Task WriteProblem(HttpContext context, HttpStatusCode status, string title, string[] errors)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)status;

        var payload = new
        {
            type = "about:blank",
            title,
            status = (int)status,
            errors
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
