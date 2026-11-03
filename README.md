# official-web-site

Бэкенд приёма заявок: .NET 9 Minimal API, JWT-авторизация админа, генерация PDF по шаблону (QuestPDF).

## Быстрый старт

Нужен [.NET SDK 9](https://dotnet.microsoft.com/download).

1. Запустить API — двойной клик по `src/ApplicationsApi/run-api.cmd`
   (или в терминале: `cd src\ApplicationsApi` → `.\run-api.cmd`).
2. Дождаться строки `Now listening on: http://localhost:5081` — Swagger откроется сам.
3. В отдельном окне получить PDF — `src/ApplicationsApi/get-pdf.cmd`.
   Скрипт создаст заявку, подаст её, войдёт админом и откроет готовый PDF.

Готовые файлы кладутся в `src/ApplicationsApi/artifacts/`:
`application-<id>.pdf` и `application-<id>.txt` (текст документа для проверки данных).

Остановить API — `Ctrl+C` в окне сервера.

## Что делает `get-pdf.cmd`

По шагам, вручную то же самое:

| Шаг | Запрос |
| --- | --- |
| Создать заявку | `POST /api/applications` |
| Подать заявку | `POST /api/applications/{id}/submit` |
| Войти админом | `POST /api/auth/login` |
| Проверить текст | `GET /api/admin/applications/{id}/preview` |
| Скачать PDF | `GET /api/admin/applications/{id}/pdf` |

Важно: PDF формируется только для поданной заявки. Для `Draft` вернётся `409`.

Для IDE есть готовый `src/ApplicationsApi/requests.http` (VS Code REST Client / Rider).

## Параметры скриптов

```powershell
# другой порт, без автооткрытия браузера
.\run-api.cmd -Port 5199 -NoBrowser

# свой файл и свои данные заявки
.\get-pdf.cmd -ApplicantName "Петров Пётр" -Phone "+375331112233" -OutFile C:\temp\zayavlenie.pdf

# скачать PDF уже существующей (поданной) заявки
.\get-pdf.cmd -ApplicationId 420a29c0-f878-4534-942f-77f1fd800c4e
```

Полный список параметров: `Get-Help .\scripts\get-pdf.ps1 -Full`.

## Конфигурация

Переменные окружения (у `run-api.cmd` есть dev-значения по умолчанию):

| Переменная | Назначение |
| --- | --- |
| `JWT_KEY` | ключ подписи токенов, минимум 32 символа (по умолчанию генерируется при запуске) |
| `ADMIN_LOGIN` / `ADMIN_PASSWORD` | учётка администратора |
| `FRONTEND_ORIGIN` | origin фронтенда для CORS, по умолчанию `http://localhost:5173` |

## Ограничения

- Заявки хранятся в памяти (`Services/IMRA.cs`) — после перезапуска всё пропадает.
- Токены живут 60 минут; при перезапуске со сгенерированным `JWT_KEY` старые токены становятся недействительны.
- PDF собирается встроенным шрифтом Lato. В QuestPDF 2026.9+ системные шрифты по умолчанию отключены, поэтому `Arial` использовать нельзя — генерация упадёт с `DocumentDrawingException`.
