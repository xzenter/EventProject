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

### BookingProcessingService

Обрабатывает бронирования со статусом `Pending` с периодичностью 1 секунда:
1. Выбирает брони с `Status == Pending`
2. Проверяет доступность мест
3. Устанавливает `Confirmed` (места есть) или `Rejected` (мест нет)
4. Заполняет `ProcessedAt`

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
# После обработки фоновым сервисом статус станет Confirmed
curl http://localhost:5002/bookings/{bookingId} \
  -H "Authorization: Bearer $TOKEN"
```