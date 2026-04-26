namespace TL.ExemploCQRS.Domain.Common;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"Entidade '{name}' com identificador '{key}' não foi encontrada.") { }
}

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Ocorreram um ou mais erros de validação.")
    {
        Errors = errors;
    }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Acesso não autorizado.") : base(message) { }
}
