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
        _next   = next;
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
        HttpStatusCode statusCode;
        object response;

        switch (exception)
        {
            case NotFoundException ex:
                statusCode = HttpStatusCode.NotFound;
                response   = ApiResponse<object?>.Fail(ex.Message);
                _logger.LogWarning("Recurso não encontrado: {Message}", ex.Message);
                break;

            case Domain.Common.ValidationException ex:
                statusCode = HttpStatusCode.BadRequest;
                // Preserva a estrutura { campo: [erros] } para que o cliente
                // saiba exatamente qual campo corrigir — não achata em lista plana.
                response = new
                {
                    success   = false,
                    message   = "Erros de validação encontrados.",
                    errors    = ex.Errors,          // IDictionary<string, string[]>
                    data      = (object?)null,
                    timestamp = DateTime.UtcNow
                };
                _logger.LogWarning("Falha de validação: {@Errors}", ex.Errors);
                break;

            case DomainException ex:
                statusCode = HttpStatusCode.BadRequest;
                response   = ApiResponse<object?>.Fail(ex.Message);
                _logger.LogWarning("Regra de negócio violada: {Message}", ex.Message);
                break;

            case UnauthorizedException ex:
                statusCode = HttpStatusCode.Unauthorized;
                response   = ApiResponse<object?>.Fail(ex.Message);
                _logger.LogWarning("Acesso não autorizado: {Message}", ex.Message);
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                response   = ApiResponse<object?>.Fail("Ocorreu um erro interno no servidor.");
                _logger.LogError(exception, "Erro não tratado: {Message}", exception.Message);
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)statusCode;

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
