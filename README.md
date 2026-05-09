# ToDoAI

`ToDoAI` — это веб-приложение для задач, расписания дня и AI-подсказок.

Состав:
- `backend` — ASP.NET Core API, auth, бизнес-логика, PostgreSQL.
- `ai-service` — FastAPI-сервис для AI-расписания и мотивационных сообщений.
- `frontend` — React + Vite клиент.

## Сервисы и порты

- Frontend: `http://localhost:5173`
- Backend Swagger: `http://localhost:5114/swagger`
- Backend Internal Swagger: `http://localhost:5114/internal/swagger`
- AI service docs: `http://localhost:8000/docs`
- Mailpit UI: `http://localhost:8025`
- PostgreSQL: `localhost:5432`

## Быстрый локальный запуск через Docker Compose

Требования:
- Docker Desktop
- доступ к Docker Hub

Из корня проекта:

```powershell
docker compose up --build
```

Это поднимет:
- `db`
- `ai-service`
- `mailpit`
- `api`
- `frontend`

Остановка:

```powershell
docker compose down
```

## Локальный запуск без Docker

Нужны:
- .NET SDK 10
- Python 3.11 + Poetry
- Node.js 24+
- локальный PostgreSQL

Порядок:

1. Поднять PostgreSQL.
2. Настроить `backend/.env`.
3. Настроить `ai-service/.env`.
4. Настроить `frontend/.env`.
5. Запустить сервисы по отдельности.

### Backend

```powershell
cd backend
dotnet restore
dotnet run --project .\ToDoAI.API\ToDoAI.API.csproj --launch-profile http
```

### AI service

```powershell
cd ai-service
poetry install
poetry run uvicorn app.server.server:app --host 0.0.0.0 --port 8000 --reload
```

### Frontend

```powershell
cd frontend
npm ci
npm run dev
```

## Docker Compose режимы

- `docker-compose.yml` — локальный dev stack с `mailpit`
- `docker-compose.prod.yml` — production stack

Для production:

```powershell
docker compose --env-file .env.prod -f docker-compose.prod.yml up --build -d
```

## Где смотреть подробнее

- [Backend README](https://github.com/beelzebufo98/ToDoAI/tree/main/backend)
- [AI service README](https://github.com/beelzebufo98/ToDoAI/tree/main/ai-service)
- [Frontend README](https://github.com/beelzebufo98/ToDoAI/tree/main/frontend)
