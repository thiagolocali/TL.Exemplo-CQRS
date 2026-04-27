namespace TL.ExemploCQRS.Application.Common;

public class ApiResponse<T>
{
    public bool Success { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public T? Data { get; private set; }
    public IEnumerable<string> Errors { get; private set; } = Enumerable.Empty<string>();
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

    protected ApiResponse() { }

    public static ApiResponse<T> Ok(T data, string message = "Operação realizada com sucesso.")
        => new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, IEnumerable<string>? errors = null)
        => new() { Success = false, Message = message, Errors = errors ?? Enumerable.Empty<string>() };
}

/// <summary>
/// Helper para respostas sem payload (ex: DELETE).
/// Retorna Data como null no JSON em vez de um objeto vazio {}.
/// </summary>
public static class ApiResponse
{
    public static ApiResponse<object?> Ok(string message = "Operação realizada com sucesso.")
        => ApiResponse<object?>.Ok(null, message);

    public static ApiResponse<object?> Fail(string message, IEnumerable<string>? errors = null)
        => ApiResponse<object?>.Fail(message, errors);
}
