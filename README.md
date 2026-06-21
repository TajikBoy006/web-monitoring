# WebMonitoring — система мониторинга доступности сайтов

Веб-сервис, который автоматически отслеживает доступность сайтов, замеряет
скорость их ответа, фиксирует HTTP-коды и присылает уведомления в Telegram
при сбоях. Аналог Uptime Robot.

## Возможности

- Добавление и удаление отслеживаемых сайтов через REST API
- Фоновая проверка всех сайтов по таймеру (интервал настраивается)
- Мгновенная проверка сайта сразу при добавлении
- Замер времени ответа (response time) и HTTP-кода
- Логирование истории всех проверок в базу данных
- Telegram-уведомления при падении и восстановлении сайта
- Веб-панель со статусами в реальном времени (онлайн / офлайн)
- Swagger-документация API

## Стек технологий

- C# / .NET 10
- ASP.NET Core (Minimal APIs)
- BackgroundService (фоновый воркер)
- HttpClient
- Entity Framework Core + SQLite
- Swagger (Swashbuckle)

## Запуск

```bash
git clone https://github.com/TajikBoy006/web-monitoring.git
cd web-monitoring
dotnet restore
dotnet run
```

После запуска:
- Веб-панель: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`

## Настройка Telegram-уведомлений (опционально)

Уведомления необязательны — основной функционал работает без них.
Чтобы включить алерты в Telegram:

1. Создайте бота через @BotFather в Telegram, получите токен
2. Напишите боту любое сообщение
3. Узнайте свой chat_id, открыв в браузере:
   `https://api.telegram.org/bot<ВАШ_ТОКЕН>/getUpdates`
4. Создайте файл `appsettings.Development.json` рядом с `appsettings.json`:

```json
{
  "Telegram": {
    "BotToken": "ваш_токен",
    "ChatId": "ваш_chat_id"
  }
}
```

## Структура проекта

​```
WebMonitoring/
├── Program.cs              # точка входа, REST API, настройка сервисов
├── appsettings.json        # конфигурация (интервал проверки)
├── Models/
│   ├── Site.cs             # модель отслеживаемого сайта
│   └── CheckLog.cs         # модель записи проверки
├── Data/
│   └── AppDbContext.cs     # контекст базы данных (EF Core)
├── Services/
│   ├── SiteChecker.cs      # проверка одного сайта через HttpClient
│   ├── TelegramNotifier.cs # отправка уведомлений в Telegram
│   └── MonitorWorker.cs    # фоновая служба проверки по таймеру
└── wwwroot/
    └── index.html          # веб-панель со статусами
​```
