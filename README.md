# EventService

EventService — учебный проект на базе ASP.NET (.NET 10),
реализованный в микросервисной архитектуре с использованием Kafka для асинхронного взаимодействия.

## Архитектура проекта

Проект разделён на три микросервиса, каждый построен по принципам Clean Architecture:

```
src/
├── EventProject.Users.Presentation/     # Микросервис пользователей и авторизации
│   ├── EventProject.Users.Application/
│   ├── EventProject.Users.Domain/
│   └── EventProject.Users.Infrastructure/
├── EventProject.Events.Presentation/    # Микросервис событий (мероприятий)
│   ├── EventProject.Events.Application/
│   ├── EventProject.Events.Domain/
│   └── EventProject.Events.Infrastructure/
├── EventProject.Bookings.Presentation/  # Микросервис бронирований
│   ├── EventProject.Bookings.Application/
│   ├── EventProject.Bookings.Domain/
│   └── EventProject.Bookings.Infrastructure/
├── EventProject.Shared/                 # Общие утилиты и контракты
└── docker-compose_all.yml              # Оркестрация всех сервисов
```

Каждый сервис имеет собственную базу данных PostgreSQL и общается с другими через Kafka.

```
┌──────────┐     ┌──────────┐     ┌──────────┐
│  Users   │     │  Events  │     │ Bookings │
│  API     │     │  API     │     │  API     │
├──────────┤     ├──────────┤     ├──────────┤
│PostgreSQL│     │PostgreSQL│     │PostgreSQL│
└──────────┘     └──────────┘     └──────────┘
       └──────────┬──────────┘
                  │ Kafka
                  ▼
         Асинхронное взаимодействие
```

## Требования

- [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/) (для запуска окружения)
- [PostgreSQL 16](https://www.postgresql.org/) (если запуск без Docker)

## Быстрый старт (Docker)

```bash
# Запуск всех сервисов, БД и Kafka
docker compose -f src/docker-compose_all.yml up -d
```

Сервисы становятся доступны:

| Сервис          | Адрес                  |
|-----------------|------------------------|
| Users API       | http://localhost:5000  |
| Events API      | http://localhost:5001  |
| Bookings API    | http://localhost:5002  |
| Kafka           | localhost:9092         |

## Настройки (конфигурация)

### Переменные окружения

При запуске через Docker Compose автоматически применяется `appsettings.Docker.json`.
При локальном запуске — `appsettings.json`.

### Рекомендации для production по безопасности

- Не храните `Jwt:Secret` в appsettings.json
- Используйте переменные окружения или внешние хранилища (Vault, Kubernetes Secrets)
- Используйте длинный случайный ключ (минимум 32–64 символа)

## Управление схемой базы данных

Схема каждой БД управляется через миграции Entity Framework Core.

```bash
# Users, находясь в проекте src/EventProject.Users.Infrastructure
dotnet ef migrations add InitialCreate

# Events, находясь в проекте src/EventProject.Events.Infrastructure
dotnet ef migrations add InitialCreate

# Bookings, находясь в проекте src/EventProject.Bookings.Infrastructure
dotnet ef migrations add InitialCreate
```

## Установка и запуск (без Docker)

```bash
# 1. Клонировать репозиторий
git clone <url>

# 2. Восстановить зависимости
dotnet restore

# 3. Собрать проекты
dotnet build src/EventProject.Users.Presentation/EventProject.Users.Presentation.csproj
dotnet build src/EventProject.Events.Presentation/EventProject.Events.Presentation.csproj
dotnet build src/EventProject.Bookings.Presentation/EventProject.Bookings.Presentation.csproj

# 4. Настроить и запустить PostgreSQL и Kafka (вручную или через Docker)

# 5. Выполнить миграции, находясь в соответствующем проекте инфраструктуры
dotnet ef database update

# 6. Запустить каждый сервис (в отдельных терминалах)
dotnet run --project src/EventProject.Users.Presentation
dotnet run --project src/EventProject.Events.Presentation
dotnet run --project src/EventProject.Bookings.Presentation
```

## Тестирование

### Unit-тесты

- xUnit + Moq
- InMemory EF Core провайдер

```bash
dotnet test
```

### Интеграционные тесты

Используют реальную PostgreSQL в Docker-контейнере. Проверяют:
- работу репозиториев с PostgreSQL
- корректность применения миграций
- взаимодействие сервисов с базой данных

Для запуска требуется Docker.

## API. Описание конечных точек

### Users API (http://localhost:5000)

| Метод  | Путь             | Описание                      |
|--------|------------------|-------------------------------|
| POST   | /auth/register   | Регистрация нового пользователя |
| POST   | /auth/login      | Вход в систему                |

### Events API (http://localhost:5001)

| Метод  | Путь          | Описание                                    |
|--------|---------------|---------------------------------------------|
| GET    | /events       | Список событий (фильтр по Title, From, To; пагинация Page/PageSize) |
| GET    | /events/{id}  | Событие по идентификатору                   |
| POST   | /events       | Создание события                            |
| PUT    | /events/{id}  | Обновление события                          |
| DELETE | /events/{id}  | Удаление события                            |

### Bookings API (http://localhost:5002)

| Метод  | Путь                | Описание                            |
|--------|---------------------|-------------------------------------|
| POST   | /events/{id}/book   | Создание бронирования              |
| GET    | /bookings/{id}      | Получение бронирования             |
| DELETE | /bookings/{id}      | Удаление бронирования              |

### Формат ошибок

При ошибках используется стандарт Problem Details (RFC 7807):

| Код | Описание |
|-----|----------|
| 400 | Ошибка валидации |
| 404 | Ресурс не найден |
| 409 | Конфликт (например, мест нет) |
| 500 | Внутренняя ошибка сервера |

## Модели данных

### User

| Поле          | Тип    | Описание              |
|---------------|--------|-----------------------|
| UserId        | Guid   | Идентификатор         |
| Login         | string | Имя пользователя      |
| PasswordHash  | string | Хеш пароля            |
| Role          | Role   | Роль (User / Admin)   |

### Event

| Поле          | Тип      | Описание                              |
|---------------|----------|---------------------------------------|
| EventId       | Guid     | Идентификатор                         |
| Title         | string   | Название                              |
| Description   | string   | Описание                              |
| StartAt       | DateTime | Начало события                        |
| EndAt         | DateTime | Окончание события                     |
| TotalSeats    | int      | Общее количество мест                 |
| AvailableSeats| int      | Доступные места (при создании = TotalSeats) |
| Bookings      | List\<Booking\> | Связанные бронирования         |

### Booking

| Поле       | Тип           | Описание                      |
|------------|---------------|-------------------------------|
| Id         | Guid          | Идентификатор                 |
| EventId    | Guid          | Идентификатор события         |
| Status     | BookingStatus | Статус (Pending / Confirmed / Rejected / Cancelled) |
| CreatedAt  | DateTime      | Время создания                |
| ProcessedAt| DateTime?     | Время обработки фоновым сервисом |
| Event      | Event         | Навигационное свойство        |

## Фоновые обработчики

### BookingProcessingService (Bookings API)

Фоновый сервис в микросервисе Bookings. Каждые 10 секунд выбирает до 50 бронирований со статусом `Pending` и обрабатывает каждое:

1. Ожидает 10 секунд (имитация обработки)
2. Меняет статус брони на `Confirmed` и сохраняет в БД Bookings
3. Публикует событие `BookingConfirmed` в топик Kafka `booking-confirmed`
4. При ошибке — меняет статус на `Rejected`

Проверка мест **не выполняется** в Bookings — эту ответственность берёт на себя Events через Kafka.

### ConsumerWorker (Events API)

Фоновый сервис в микросервисе Events. Слушает топик Kafka `booking-confirmed` и обрабатывает сообщения `BookingConfirmed`:

**BookingConfirmed** (EventProject.Shared):

| Поле        | Тип      | Описание                     |
|-------------|----------|------------------------------|
| BookingId   | Guid     | Идентификатор брони          |
| EventId     | Guid     | Идентификатор события        |
| UserId      | Guid     | Идентификатор пользователя   |
| Seats       | int      | Количество мест (всегда 1)   |
| ConfirmedAt | DateTime | Время подтверждения          |

При получении сообщения ConsumerWorker:

1. Загружает событие (`Event`) из БД Events по `EventId`
2. Проверяет, что событие существует, не началось и есть достаточно мест
3. Вызывает `event.TryReserveSeats(seats)` — уменьшает `AvailableSeats`
4. Сохраняет изменения в БД Events
5. Фиксирует смещение (offset) в Kafka (только после успешного резервирования)

При неудаче (событие не найдено / уже началось / нет мест) — логирует предупреждение и **не фиксирует** offset,
сообщение будет прочитано повторно после перезапуска.

### Полный поток BookingConfirmed

```
Клиент
  │
  │ POST /events/{id}/book
  ▼
BookingsService.CreateBooking
  │ 1. Создаёт Booking (Status = Pending)
  │ 2. Сохраняет в БД Bookings
  ▼
Bookings DB (Pending)
  │
  │ (каждые 10 сек)
  ▼
BookingProcessingService
  │ 1. Выбирает Pending брони
  │ 2. Подтверждает (Status = Confirmed)
  │ 3. Сохраняет в БД Bookings
  │ 4. Публикует BookingConfirmed в Kafka
  ▼
Kafka topic "booking-confirmed"
  │
  │ (асинхронно)
  ▼
ConsumerWorker (Events API)
  │ 1. Читает BookingConfirmed из Kafka
  │ 2. Загружает Event из БД Events
  │ 3. Проверяет: событие существует, не началось, есть места
  │ 4. Резервирует места (AvailableSeats -= Seats)
  │ 5. Сохраняет в БД Events
  │ 6. Фиксирует offset
  ▼
Events DB (AvailableSeats обновлено)
```

## Аутентификация и авторизация (JWT)

### Получение токена

```bash
# Регистрация
curl -X POST http://localhost:5000/auth/register \
  -H "Content-Type: application/json" \
  -d '{"login": "test", "password": "password"}'

# Вход
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"login": "test", "password": "password"}'
```

### Использование токена

```http
Authorization: Bearer <JWT-токен>
```

### Ролевая модель

| Роль  | Права                                              |
|-------|----------------------------------------------------|
| User  | Просмотр событий, создание/просмотр/отмена своих броней |
| Admin | Полный доступ, CRUD событий, управление всеми бронями   |

## Пример сценария

```bash
# 1. Регистрация и вход (Users API)
TOKEN=$(curl -s -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"login":"admin","password":"admin"}' | jq -r '.token')

# 2. Создать событие (Events API)
EVENT_ID=$(curl -s -X POST http://localhost:5001/events \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"title":"Концерт","description":"Описание","startAt":"2026-12-01T18:00:00","endAt":"2026-12-01T22:00:00","totalSeats":100}' | jq -r '.eventId')

# 3. Забронировать место (Bookings API)
curl -X POST "http://localhost:5002/events/$EVENT_ID/book" \
  -H "Authorization: Bearer $TOKEN"

# 4. Получить бронь (Bookings API)
# Статус станет Confirmed (через ~20 сек), места уменьшатся в Events API
curl http://localhost:5002/bookings/{bookingId} \
  -H "Authorization: Bearer $TOKEN"
```

## Стратегия кеширования

Для уменьшения нагрузки на базу данных и ускорения обработки запросов в сервисе `EventProject.Events` используется `Redis`.

В `Redis` кешируются следующие данные:

- информация о событии при получении по идентификатору (GET `/events/{id}`);
- список из 10 популярных событий (GET `/events/top`).

Данные выбираются для кеширования потому, что значительно чаще читаются, чем изменяются.
Использование `Redis` позволяет сократить количество обращений к базе данных и уменьшить время ответа `API`.

Используется стратегия `Cache-Aside` для операций чтения:

1. При запросе данных сервис сначала пытается получить их из `Redis`.
2. Если запись найдена, она возвращается клиенту без обращения к базе данных.
3. При отсутствии записи в кеше данные загружаются из базы данных, после чего сохраняются в `Redis` с установленным временем жизни `TTL`.

При изменении данных используется стратегия `Update-on-Write`:

- после успешного обновления события соответствующая запись в `Redis` немедленно обновляется;
- благодаря этому последующие запросы получают актуальные данные без необходимости повторного чтения из базы данных.

Для записей в кеше используется ограниченное время жизни `TTL`, которое настраивается через конфигурацию приложения.

Использование `TTL` позволяет:

- автоматически удалять редко используемые записи;
- предотвращать длительное хранение устаревших данных;
- ограничивать объём занимаемой памяти `Redis`.

Даже если запись не была обновлена вручную, после истечения `TTL` она будет автоматически удалена и при следующем обращении заново загружена из базы данных.

`Redis` используется исключительно как дополнительный уровень хранения данных и не является критически важным компонентом системы.

Если `Redis` недоступен:

- чтение данных выполняется напрямую из базы данных;
- приложение продолжает работать без потери функциональности;
- ошибки работы с `Redis` не приводят к отказу `API`.

Изменение количества свободных мест происходит асинхронно после обработки сообщений `Kafka` сервисами бронирования.

После успешной обработки сообщений (`BookingConfirmed`) сервис обновляет информацию о событии и сразу записывает актуальное состояние в `Redis`  (`Update-on-Write`).

Такой подход позволяет:

- поддерживать согласованность между базой данных и `Redis`;
- минимизировать вероятность чтения устаревших данных;
- избежать необходимости принудительной очистки кеша после каждого изменения.



## Наблюдаемость (Observability)

Для мониторинга состояния системы, сбора метрик, анализа производительности и трассировки запросов добавлен стек наблюдаемости:

Используемые инструменты:

### Prometheus

Используется для сбора и хранения метрик приложений через `OpenTelemetry Metrics`.
В каждом микросервисе настроен `OpenTelemetry SDK`, который публикует метрики через Prometheus exporter.

Собираемые метрики:

- метрики HTTP запросов ASP.NET Core
- метрики .NET Runtime
- метрики ASP.NET Core
- пользовательские метрики приложения (при добавлении собственных Meter/Counter/Histogram).

Метрики публикуются каждым API сервисом через endpoint: `/metrics`

Prometheus периодически выполняет scraping этого endpoint и сохраняет полученные значения.

UI Prometheus доступен: `http://localhost:9090`
Проверка состояния подключенных сервисов: `http://localhost:9090/targets`

### Grafana

Используется для визуализации метрик и создания дашбордов.

Добавлены дашборды для:

- длительность обработки HTTP запросов
- текущее количество HTTP запросов, находящихся в обработке
- количество обработанных HTTP запросов
- работа Garbage Collector
- загрузка процессора CPU
- использования памяти процесса RAM

UI Grafana доступен: `http://localhost:3000`

Данные для подключения:
- Login: admin
- Password: admin

Grafana использует Prometheus как источник данных (Datasource).

### Jaeger

Используется для распределённой трассировки запросов между микросервисами.
Трассировка реализована через `OpenTelemetry Tracing`.

Собираются:

- входящие HTTP запросы ASP.NET Core;
- исходящие HTTP запросы через HttpClient;
- обращения к базе данных через Entity Framework Core;
- длительность выполнения операций;
- ошибки при обработке запросов.

Трейсы отправляются через OTLP exporter в Jaeger.

Системные endpoint-ы `/health` и `/metrics` исключены из трассировки, чтобы не создавать лишний шум в Jaeger.

UI Jaeger доступен: `http://localhost:16686`
