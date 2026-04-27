using Microsoft.OpenApi.Models;
using System.Reflection;
using TL.ExemploCQRS.Application;
using TL.ExemploCQRS.Infrastructure;
using TL.ExemploCQRS.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// ── Camadas ───────────────────────────────────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── Controllers ───────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = false;
    });

// ── CORS ──────────────────────────────────────────────────────────────────────
// Em desenvolvimento: permissivo para facilitar testes locais.
// Em produção: restrito às origens configuradas em "AllowedOrigins" no appsettings.
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

    var allowedOrigins = builder.Configuration
        .GetSection("AllowedOrigins")
        .Get<string[]>() ?? [];

    options.AddPolicy("Production", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// ── Swagger / OpenAPI ─────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "TL.Exemplo-CQRS API",
        Version     = "v1",
        Description = "API de exemplo utilizando CQRS com MediatR, FluentValidation, EF Core e JWT.",
        Contact     = new OpenApiContact { Name = "TechLead", Email = "contato@tl-exemplo.com" }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name        = "Authorization",
        Type        = SecuritySchemeType.Http,
        Scheme      = "Bearer",
        BearerFormat = "JWT",
        In          = ParameterLocation.Header,
        Description = "Informe o token JWT no formato: Bearer {seu_token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// ── Authorization ─────────────────────────────────────────────────────────────
builder.Services.AddAuthorization();

// ── HealthCheck ───────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

var app = builder.Build();

// ── Middlewares ───────────────────────────────────────────────────────────────
app.UseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TL.Exemplo-CQRS API v1");
        c.RoutePrefix     = string.Empty;
        c.DocumentTitle   = "TL.Exemplo-CQRS API";
        c.DisplayRequestDuration();
    });

    app.UseCors("Development");
}
else
{
    app.UseCors("Production");
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// ── Migrações automáticas em desenvolvimento ──────────────────────────────────
if (app.Environment.IsDevelopment())
{
    try
    {
        await app.Services.ApplyMigrationsAsync();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex,
            """
            ╔══════════════════════════════════════════════════════════════╗
            ║  FALHA AO APLICAR MIGRATIONS — verifique a connection string ║
            ╠══════════════════════════════════════════════════════════════╣
            ║  Edite: appsettings.Development.json                        ║
            ║                                                              ║
            ║  Opção 1 — SQL Server local com Windows Auth:               ║
            ║  Server=localhost;Database=TLExemploCQRS_Dev;               ║
            ║  Trusted_Connection=True;TrustServerCertificate=True;       ║
            ║                                                              ║
            ║  Opção 2 — SQL Server com usuário/senha (SA):               ║
            ║  Server=localhost;Database=TLExemploCQRS_Dev;               ║
            ║  User Id=sa;Password=SuaSenha@123;                          ║
            ║  TrustServerCertificate=True;                               ║
            ║                                                              ║
            ║  Opção 3 — Docker:                                          ║
            ║  docker run -e ACCEPT_EULA=Y -e SA_PASSWORD=SuaSenha@123   ║
            ║    -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest║
            ╚══════════════════════════════════════════════════════════════╝
            """);
    }
}

app.Run();
