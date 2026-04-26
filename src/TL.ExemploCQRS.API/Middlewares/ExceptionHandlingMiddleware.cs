using System.Net;
using System.Text.Json;
using TL.ExemploCQRS.Application.Common;
using TL.ExemploCQRS.Domain.Common;

namespace TL.ExemploCQRS.API.Middlewares;

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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            NotFoundException ex => (
                HttpStatusCode.NotFound,
                ApiResponse<object>.Fail(ex.Message)),

            Domain.Common.ValidationException ex => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail(
                    "Erros de validação encontrados.",
                    ex.Errors.SelectMany(e => e.Value))),

            DomainException ex => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail(ex.Message)),

            UnauthorizedException ex => (
                HttpStatusCode.Unauthorized,
                ApiResponse<object>.Fail(ex.Message)),

            _ => (
                HttpStatusCode.InternalServerError,
                ApiResponse<object>.Fail("Ocorreu um erro interno no servidor."))
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Erro não tratado: {Message}", exception.Message);
        else
            _logger.LogWarning(exception, "Erro de negócio: {Message}", exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}
