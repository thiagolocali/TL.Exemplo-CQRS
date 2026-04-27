using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TL.ExemploCQRS.Application.Common.Behaviors;

namespace TL.ExemploCQRS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddValidatorsFromAssembly(assembly);
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // Ordem importa: o MediatR executa os behaviors na sequência de registro.
            // ValidationBehavior primeiro → rejeita requests inválidos antes de logar
            // como erro. LoggingBehavior segundo → envolve apenas handlers que passaram
            // na validação, evitando ruído de ValidationException nos logs de erro.
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        });

        return services;
    }
}
