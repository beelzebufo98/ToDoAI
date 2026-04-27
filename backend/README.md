# Backend

`backend` — это ASP.NET Core API для `ToDoAI`.

Что делает:
- регистрация, логин, refresh token flow
- подтверждение email и сброс пароля
- CRUD задач
- task execution и дневное расписание
- интеграция с `ai-service`
- отправка email через SMTP

## Стек

- .NET 10
- ASP.NET Core
- Entity Framework Core
- PostgreSQL

## Основные точки входа

- Swagger: `http://localhost:5114/swagger`
- Internal Swagger: `http://localhost:5114/internal/swagger`

## Локальный запуск

Требования:
- .NET SDK 10
- доступная PostgreSQL

Подготовка:
- заполнить `backend/.env`
- при локальном SMTP можно использовать `Mailpit`
- при реальном SMTP можно использовать `appsettings.Development.json`

Команды:

```powershell
dotnet restore
dotnet run --project .\ToDoAI.API\ToDoAI.API.csproj --launch-profile http
```

API поднимется на:

```text
http://localhost:5114
```

## Docker

Изолированный backend stack:

```powershell
docker compose up --build
```

Файл:
- `backend/docker-compose.yml`

Этот режим поднимает:
- `db`
- `ai-service`
- `mailpit`
- `api`

## Конфиг

Ключевые настройки backend:
- `ConnectionStrings__Default`
- `AuthSettings__SecretKey`
- `AuthSettings__AccessTokenLifetime`
- `AuthSettings__RefreshTokenLifetime`
- `AiService__Enabled`
- `AiService__BaseUrl`
- `AiService__GenerateSchedulePath`
- `AiService__GenerateMotivationPath`
- `EmailSettings__Enabled`
- `EmailSettings__Host`
- `EmailSettings__Port`
- `EmailSettings__SocketSecurityMode`
- `EmailSettings__UserName`
- `EmailSettings__Password`
- `EmailSettings__FromAddress`
- `EmailSettings__FromName`

## Полезно

- dev email endpoint доступен только в `Development`
- для локальной проверки писем удобнее использовать `Mailpit`
- AI-запросы backend отправляет не напрямую в OpenRouter, а в `ai-service`
