# Frontend

`frontend` — это React-клиент для `ToDoAI`.

Что делает:
- флоу регистрация и логина
- работа с задачами и обратной связью по задачам
- генерация и показ расписания
- показ AI советов при создании задачи и мотивационных сообщений

## Стек

- React 19
- Vite
- TypeScript
- React Router
- React Query
- Tailwind

## Локальный запуск

Требования:
- Node.js 24+
- npm

Подготовка:
- заполнить `frontend/.env`
- обычно достаточно:

```env
VITE_API_URL=http://localhost:5114/api/v1.0
```

Команды:

```powershell
npm ci
npm run dev
```

Приложение будет доступно на:

```text
http://localhost:5173
```

## Сборка

```powershell
npm run build
```

Preview:

```powershell
npm run preview
```

## Docker

Сборка идет через:
- `frontend/Dockerfile`

В local compose frontend публикуется на:

```text
http://localhost:5173
```

Во время docker build API URL прокидывается через `VITE_API_URL`.
