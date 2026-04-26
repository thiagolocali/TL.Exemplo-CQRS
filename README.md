# TL.Exemplo-CQRS

Projeto de exemplo utilizando **CQRS** (Command Query Responsibility Segregation) com ASP.NET Core 8.

## 🏗️ Arquitetura

```
TL.Exemplo-CQRS/
├── src/
│   ├── TL.ExemploCQRS.Domain/          # Entidades, interfaces, exceções de domínio
│   ├── TL.ExemploCQRS.Application/     # Commands, Queries, DTOs, Validators, Behaviors
│   ├── TL.ExemploCQRS.Infrastructure/  # EF Core, Repositories, JWT, PasswordHasher
│   └── TL.ExemploCQRS.API/             # Controllers, Middlewares, Program.cs
└── tests/
    ├── TL.ExemploCQRS.Tests.Unit/
    └── TL.ExemploCQRS.Tests.Integration/
```

## ✅ Tecnologias

| Pacote | Função |
|--------|--------|
| **MediatR 12** | CQRS pipeline (Commands/Queries) |
| **FluentValidation 11** | Validação de requests |
| **AutoMapper 13** | Mapeamento entre camadas |
| **Entity Framework Core 8** | ORM com SQL Server |
| **Swashbuckle 6** | Swagger/OpenAPI |
| **JWT Bearer** | Autenticação com tokens JWT |

## ⚙️ Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (local ou Docker)

### SQL Server com Docker (opcional)
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=SuaSenha@123" \
  -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

## 🚀 Como rodar

### 1. Configure a connection string

Edite `src/TL.ExemploCQRS.API/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TLExemploCQRS_Dev;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

> **Usuário SA do Docker:** `Server=localhost;Database=TLExemploCQRS_Dev;User Id=sa;Password=SuaSenha@123;TrustServerCertificate=True;`

### 2. Execute as migrações (automáticas em desenvolvimento)

As migrações são aplicadas automaticamente ao iniciar a aplicação em `Development`.

Ou manualmente:
```bash
cd src/TL.ExemploCQRS.API
dotnet ef database update --project ../TL.ExemploCQRS.Infrastructure
```

### 3. Rode a aplicação

```bash
cd src/TL.ExemploCQRS.API
dotnet run
```

Acesse: **http://localhost:5000** (Swagger na raiz)

## 🔐 Autenticação JWT

### 1. Registrar usuário
```http
POST /api/v1/auth/register
{
  "username": "admin",
  "email": "admin@exemplo.com",
  "password": "Admin@123",
  "confirmPassword": "Admin@123"
}
```

### 2. Fazer login
```http
POST /api/v1/auth/login
{
  "username": "admin",
  "password": "Admin@123"
}
```

### 3. Usar o token
No Swagger, clique em **Authorize** e informe: `Bearer {seu_token}`

Nos requests HTTP:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

## 📦 Endpoints - Products CRUD

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| GET | `/api/v1/products` | Não | Lista produtos (paginado) |
| GET | `/api/v1/products/{id}` | Não | Busca por ID |
| POST | `/api/v1/products` | Sim | Cria produto |
| PUT | `/api/v1/products/{id}` | Sim | Atualiza produto |
| DELETE | `/api/v1/products/{id}` | Sim | Remove (soft delete) |

### Query Params - GET /products
| Parâmetro | Tipo | Padrão | Descrição |
|-----------|------|--------|-----------|
| `page` | int | 1 | Página atual |
| `pageSize` | int | 10 | Itens por página |
| `search` | string | - | Busca por nome/descrição |
| `isActive` | bool | - | Filtrar por status |

## 📐 Padrão de Response

Todos os endpoints retornam o envelope `ApiResponse<T>`:

```json
{
  "success": true,
  "message": "Operação realizada com sucesso.",
  "data": { ... },
  "errors": [],
  "timestamp": "2024-01-01T00:00:00Z"
}
```

### Erros tratados automaticamente

| HTTP | Exceção | Situação |
|------|---------|----------|
| 400 | `ValidationException` | Falhas de validação FluentValidation |
| 400 | `DomainException` | Regra de negócio violada |
| 401 | `UnauthorizedException` | Login inválido |
| 404 | `NotFoundException` | Recurso não encontrado |
| 500 | `Exception` | Erro interno (sem detalhes expostos) |

## 🔄 Fluxo CQRS

```
HTTP Request
    ↓
Controller
    ↓
MediatR.Send(Command/Query)
    ↓
LoggingBehavior (pipeline)
    ↓
ValidationBehavior (pipeline) → FluentValidation
    ↓
CommandHandler / QueryHandler
    ↓
Repository → EF Core → SQL Server
    ↓
Response (DTO)
```

## 🗄️ Estrutura do Banco

### Tabela Products
| Coluna | Tipo | Descrição |
|--------|------|-----------|
| Id | UNIQUEIDENTIFIER | PK (GUID gerado na aplicação) |
| Name | NVARCHAR(150) | Nome do produto |
| Description | NVARCHAR(500) | Descrição |
| Price | DECIMAL(18,2) | Preço |
| StockQuantity | INT | Quantidade em estoque |
| IsActive | BIT | Status ativo/inativo |
| IsDeleted | BIT | Soft delete |
| CreatedAt | DATETIME2 | Data de criação |
| UpdatedAt | DATETIME2 | Data de atualização |

### Tabela Users
| Coluna | Tipo | Descrição |
|--------|------|-----------|
| Id | UNIQUEIDENTIFIER | PK |
| Username | NVARCHAR(50) | Usuário único |
| Email | NVARCHAR(200) | E-mail único |
| PasswordHash | NVARCHAR(MAX) | Hash PBKDF2-SHA256 |
| Role | NVARCHAR(50) | Papel (User/Admin) |
| IsDeleted | BIT | Soft delete |

## 🔧 Configuração JWT (appsettings.json)

```json
{
  "Jwt": {
    "SecretKey": "TL-CQRS-SuperSecretKey-2024-MinLength32Chars!",
    "Issuer": "TL.ExemploCQRS",
    "Audience": "TL.ExemploCQRS.Client",
    "ExpirationHours": "8"
  }
}
```

> ⚠️ Em produção, use variáveis de ambiente ou Azure Key Vault para o `SecretKey`.

## 🛠️ Comandos úteis

```bash
# Criar nova migration
dotnet ef migrations add NomeDaMigration \
  --project src/TL.ExemploCQRS.Infrastructure \
  --startup-project src/TL.ExemploCQRS.API

# Aplicar migrations
dotnet ef database update \
  --project src/TL.ExemploCQRS.Infrastructure \
  --startup-project src/TL.ExemploCQRS.API

# Build do projeto completo
dotnet build TL.Exemplo-CQRS.sln

# Rodar testes
dotnet test TL.Exemplo-CQRS.sln
```
