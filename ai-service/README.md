# AI Service

`ai-service` — это FastAPI-сервис, через который backend получает AI-ответы.

Что делает:
- генерация дневного расписания
- генерация коротких мотивационных сообщений
- работа с OpenRouter-моделями

## Стек

- Python 3.11
- FastAPI
- Uvicorn
- Poetry
- HTTPX

## Эндпоинты

- `POST /api/v1/ai/schedule/generate`
- `POST /api/v1/ai/motivation/generate`

Docs:

```text
http://localhost:8000/docs
```

## Локальный запуск

Требования:
- Python 3.11
- Poetry

Подготовка:
- заполнить `ai-service/.env`

Команды:

```powershell
poetry install
poetry run uvicorn app.server.server:app --host 0.0.0.0 --port 8000 --reload
```

## Ключевые переменные

Смотри `ai-service/.env.example`.

Основные:
- `APP_PORT`
- `WORKERS`
- `RELOAD`
- `OPENROUTER_API_KEY`
- `OPENROUTER_BASE_URL`
- `OPENROUTER_UX_MODEL`
- `OPENROUTER_PLANNING_MODEL`
- `OPENROUTER_TIMEOUT_SECONDS`
- `OPENROUTER_HTTP_REFERER`
- `OPENROUTER_APP_TITLE`

## Docker

Сервис собирается через:
- `ai-service/docker/Dockerfile`

В локальном compose он стартует внутри общего stack и доступен backend по адресу:

```text
http://ai-service:8000
```

## Тесты

Запуск unit-тестов:

```powershell
poetry run pytest
```
