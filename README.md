# dotnet-webapi-auth-boilerplate

[![CI](https://github.com/osisdie/dotnet-webapi-auth-boilerplate/actions/workflows/ci.yml/badge.svg)](https://github.com/osisdie/dotnet-webapi-auth-boilerplate/actions/workflows/ci.yml)
[![.NET 8](https://img.shields.io/badge/.NET-8.0%20LTS-512bd4)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512bd4)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

Production-ready .NET WebAPI with JWT authentication, refresh tokens, Dapper ORM, and Redis caching — powered by [CoreFX SDK](https://github.com/osisdie/dotnet-corefx-sdk).

## Features

- **JWT Authentication** — Login, access tokens (60 min), refresh tokens (180 min)
- **Multi-target** — Supports both .NET 8.0 LTS and .NET 10.0
- **Redis Caching** — `IDistributedCache` with StackExchange.Redis and failback scoring
- **Dapper ORM** — Async database access with SQL Server
- **Log4net Logging** — Console, file, and AWS CloudWatch support
- **Middleware Pipeline** — Exception handling, JWT authorization, request/response logging
- **Swagger/OpenAPI** — Versioned API docs (v202603, v202104, v202103)
- **Health Checks** — Built-in `/health` endpoint
- **Docker & K8s** — Multi-stage Dockerfile and minikube manifests
- **Configuration** — Environment-based (Debug/Development/Testing/Staging/Production)
- **FluentValidation** — Request data validation
- **xUnit Testing** — Unit and integration test projects

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Redis](https://redis.io/) (optional, for caching features)
- [Docker](https://www.docker.com/) (optional, for container deployment)

## Quick Start

```bash
# Clone
git clone https://github.com/osisdie/dotnet-webapi-auth-boilerplate.git
cd dotnet-webapi-auth-boilerplate

# Set required env var
export COREFX_API_NAME=auth-dev

# Build & Run
dotnet build auth-api-all-projects.sln
dotnet run --project src/Endpoint/Hello6/Hello6.Domain.Endpoint.csproj

# Browse
# http://localhost:5006/swagger
# http://localhost:5006/health
```

## Projects

**Hello6.Domain.Endpoint** is the primary web application wrapping all dependent projects:

- **CoreFX** — Shared framework libraries
  - CoreFX.Abstractions, CoreFX.Common, CoreFX.Hosting
  - CoreFX.Auth, CoreFX.Caching.Redis, CoreFX.Logging.Log4net
  - CoreFX.Notification.Smtp

- **Hello6** — Domain-driven design services
  - Hello6.Domain.Common, Hello6.Domain.Contract
  - Hello6.Domain.DataAccess.Database, Hello6.Domain.SDK

## API Endpoints

| Endpoint | Description |
|----------|-------------|
| `GET /health` | Health check |
| `GET /api/echo/ver` | Service version |
| `GET /api/echo/config` | Configuration status |
| `GET /api/echo/db` | Database connection check |
| `GET /api/echo/cache` | Redis cache check |
| `POST /api/auth/login` | JWT login |
| `POST /api/auth/refreshtoken` | Refresh JWT token |

## Docker

```bash
export VERSION=$(cat src/Endpoint/Hello6/.version | head -n1)
export IMAGE_HOST=docker.io/[ACCOUNT-ID]

bash dockerbuild.sh
```

## Testing

```bash
# Unit tests
dotnet test auth-api-all-projects.sln -c Release

# Integration test (requires running service)
export ASPNETCORE_ENVIRONMENT=Development
export CI_TEST_ENDPOINT=http://localhost:5006
dotnet test tests/IntegrationTest/Hello6/IntegrationTest.Hello6.csproj -c Release
```

## See Also

- [dotnet-corefx-sdk](https://github.com/osisdie/dotnet-corefx-sdk) — The reusable CoreFX framework
- [dotnet-minikube-boilerplate](https://github.com/osisdie/dotnet-minikube-boilerplate) — K8s deployment focused
- [dotnet-mediatr-boilerplate](https://github.com/osisdie/dotnet-mediatr-boilerplate) — MediatR/CQRS focused

## License

[MIT](LICENSE)
