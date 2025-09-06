# MyApp Backend — ASP.NET Core 8 + MySQL + JWT + Docker

Готовый шаблон бэкенда для быстрых стартов:

* **ASP.NET Core 8 Web API** (C#)
* **MySQL 8** + **EF Core (Pomelo)**
* **ASP.NET Core Identity** + **JWT** (регистрация/логин)
* **CRUD пример: Posts** (GET публично, POST/PUT/DELETE под JWT)
* **Безопасность по умолчанию**: CORS, security headers, rate limiting
* **Swagger UI** с поддержкой Authorize (Bearer JWT)
* **Docker Compose**: API + MySQL + **phpMyAdmin** (веб‑UI для БД)

---

## Содержание

1. [Быстрый старт (TL;DR)](#быстрый-старт-tldr)
2. [Что входит в проект](#что-входит-в-проект)
3. [Предпосылки](#предпосылки)
4. [Склонировать или скачать](#склонировать-или-скачать)
5. [Настроить переменные окружения](#настроить-переменные-окружения)
6. [Запуск в Docker](#запуск-в-docker)
7. [Проверка работоспособности](#проверка-работоспособности)
8. [Тесты API (через Swagger и curl)](#тесты-api-через-swagger-и-curl)
9. [Доступ к базе данных (phpMyAdmin)](#доступ-к-базе-данных-phpmyadmin)
10. [Настройка CORS](#настройка-cors)
11. [JWT: issuer/audience/secret](#jwt-issueraudiencesecret)
12. [Переключение на EF Migrations (рекомендуется для прод)](#переключение-на-ef-migrations-рекомендуется-для-прод)
13. [Структура проекта](#структура-проекта)
14. [Расширение API: как добавить новую сущность](#расширение-api-как-добавить-новую-сущность)
15. [Подсказки по продакшену](#подсказки-по-продакшену)
16. [Устранение неполадок](#устранение-неполадок)
17. [Полезные команды Docker](#полезные-команды-docker)

---

## Быстрый старт (TL;DR)

```bash
# 1) Клонируй репозиторий или скачай архив
# 2) Создай .env из шаблона
cp .env.example .env
# 3) Отредактируй .env (минимум JWT_SECRET, ADMIN_* при необходимости)
# 4) Подними контейнеры
docker compose up --build -d
# 5) Открой
# API (Swagger):     http://localhost:8080/swagger
# phpMyAdmin (БД):   http://localhost:8082
```

---

## Что входит в проект

* `src/MyApp.Api` — Web API на .NET 8
* **Auth**: `POST /api/auth/register`, `POST /api/auth/login` → выдают JWT
* **Posts**:

   * Публично: `GET /api/posts`, `GET /api/posts/{id}`
   * Под JWT: `POST /api/posts`, `PUT /api/posts/{id}`, `DELETE /api/posts/{id}`
* Конфиг безопасности: CORS, security headers, rate limiting
* Здоровье сервиса: `GET /healthz`
* Docker Compose: MySQL + phpMyAdmin + API

---

## Предпосылки

* **Docker Desktop** или **Docker Engine + docker compose** (v2)
* Интернет‑доступ для скачивания образов

*Опционально для локальной разработки без Docker:*

* .NET 8 SDK
* Локальный MySQL Server

---

## Склонировать или скачать

**Вариант A — Git**

```bash
git clone https://github.com/Tiltushkin/Tiltushkin-API.git
cd Tiltushkin-API
```

**Вариант B — ZIP**

1. Нажми **Code → Download ZIP** на странице репозитория
2. Распакуй архив
3. Перейди в папку проекта в терминале

---

## Настроить переменные окружения

Сделай рабочий `.env` на основе шаблона:

```bash
cp .env.example .env
```

Открой `.env` и проверь/измени ключевые параметры:

| Переменная                                          | Назначение                                                         | Пример                                         |
| --------------------------------------------------- | ------------------------------------------------------------------ | ---------------------------------------------- |
| `MYSQL_DATABASE`                                    | имя БД                                                             | `myapp`                                        |
| `MYSQL_USER` / `MYSQL_PASSWORD`                     | учётка БД                                                          | `myapp` / `devpass123`                         |
| `MYSQL_ROOT_PASSWORD`                               | пароль root                                                        | `rootpass123`                                  |
| `JWT_SECRET`                                        | **секрет для подписи JWT** (длинная случайная строка)              | `openssl rand -base64 64`                      |
| `JWT_ISSUER`                                        | «кто выдал токен»                                                  | `MyApp` или `https://api.example.com`          |
| `JWT_AUDIENCE`                                      | «для кого токен»                                                   | `MyAppAudience` или `https://example.com`      |
| `CORS_ALLOWED_ORIGINS`                              | список фронтов через запятую                                       | `http://localhost:3000,http://localhost:5173`  |
| `ADMIN_EMAIL` / `ADMIN_USERNAME` / `ADMIN_PASSWORD` | админ, который создастся при **первом** старте БД (если БД пустая) | `admin@example.com` / `admin` / `ChangeMe123!` |

> **Важно:** `.env` должен находиться **в корне** проекта рядом с `docker-compose.yml`. Добавь `.env` в `.gitignore`.

*Сгенерировать секрет быстро:*

* **macOS/Linux**: `openssl rand -base64 64`
* **Windows PowerShell**:

  ```powershell
  $b = New-Object byte[] 64; [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b); [Convert]::ToBase64String($b)
  ```

---

## Запуск в Docker

```bash
docker compose up --build -d
```

Контейнеры и порты по умолчанию:

| Сервис     | URL/порт хоста                      | Назначение                              |
| ---------- | ----------------------------------- | --------------------------------------- |
| API        | `http://localhost:8080`             | Web API + Swagger                       |
| phpMyAdmin | `http://localhost:8082`             | UI для работы с MySQL                   |
| MySQL      | `localhost:3307` → контейнер `3306` | Доступ к MySQL с хоста (не обязательно) |

Проверить статус:

```bash
docker compose ps
```

Остановить:

```bash
docker compose down
```

Сбросить с удалением БД (volume):

```bash
docker compose down -v
```

---

## Проверка работоспособности

* **Health‑check**: `GET http://localhost:8080/healthz` → `{"status":"ok"}`
* **Swagger**: `http://localhost:8080/swagger`

Если Swagger открылся — API запущен.

---

## Тесты API (через Swagger и curl)

### 1) Получить JWT

**Вариант A — логин админом** (если заполнили `ADMIN_*` и БД создавалась впервые):

* `POST /api/auth/login`

  ```json
  { "email": "admin@example.com", "password": "ChangeMe123!" }
  ```

**Вариант B — зарегистрировать обычного пользователя**

* `POST /api/auth/register`

  ```json
  { "email": "you@example.com", "username": "you", "password": "StrongPass123" }
  ```

В ответе будет `token`. В Swagger нажмите **Authorize** и вставьте:

```
Bearer <token>
```

### 2) CRUD Posts

* Публично:

   * `GET /api/posts`
   * `GET /api/posts/{id}`
* Под JWT:

   * `POST /api/posts`
   * `PUT /api/posts/{id}`
   * `DELETE /api/posts/{id}`

**Примеры (curl):**

```bash
# Создать пост (нужен JWT)
curl -X POST http://localhost:8080/api/posts \
  -H "Authorization: Bearer <TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{"title":"Первый пост","content":"Привет, мир!","author":"Vlada"}'

# Список постов (публично)
curl http://localhost:8080/api/posts

# Обновить пост (нужен JWT)
curl -X PUT http://localhost:8080/api/posts/1 \
  -H "Authorization: Bearer <TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{"title":"Обновили","content":"Новый текст","author":"Vlada"}'

# Удалить пост (нужен JWT)
curl -X DELETE http://localhost:8080/api/posts/1 \
  -H "Authorization: Bearer <TOKEN>"
```

---

## Доступ к базе данных (phpMyAdmin)

* URL: `http://localhost:8082`
* Server: `mysql`
* Username/Password: из `.env` (`MYSQL_USER` / `MYSQL_PASSWORD`)
* При необходимости root‑доступ: `root` / `MYSQL_ROOT_PASSWORD`

> Для продакшена **не** публикуйте phpMyAdmin наружу. Ограничьте доступ (VPN/SSH‑туннель/фаервол/Basic Auth через реверс‑прокси).

---

## Настройка CORS

В `.env` укажите домены фронтенда:

```
CORS_ALLOWED_ORIGINS=http://localhost:3000,http://localhost:5173
```

Без пробелов, через запятую. Пример для нескольких прод‑доменов:

```
CORS_ALLOWED_ORIGINS=https://app.example.com,https://admin.example.com
```

Перезапустите контейнеры после изменения `.env`.

---

## JWT: issuer/audience/secret

* `JWT_SECRET` — **секрет** подписи токенов (держите в тайне, длинный random)
* `JWT_ISSUER` — «кто выдал токен» (зачастую URL API)
* `JWT_AUDIENCE` — «для кого токен» (ресурс/клиент или URL фронта)

В проверке токена (`TokenValidationParameters`) эти значения должны совпасть с теми, что использовались при выпуске.

---

## Переключение на EF Migrations (рекомендуется для прод)

По умолчанию для скорости используется `EnsureCreated()`.

1. Установите инструменты EF (один раз):

   ```bash
   dotnet tool install --global dotnet-ef
   ```
2. Локально (в папке `src/MyApp.Api`) создайте миграцию:

   ```bash
   dotnet ef migrations add InitialCreate
   ```
3. Примените миграцию локально:

   ```bash
   dotnet ef database update
   ```
4. В `Program.cs` замените

   ```csharp
   await db.Database.EnsureCreatedAsync();
   ```

   на

   ```csharp
   await db.Database.MigrateAsync();
   ```
5. Пересоберите Docker‑образы:

   ```bash
   docker compose down
   docker compose up --build -d
   ```

> Альтернатива: организовать запуск `dotnet ef database update` в отдельном build‑шаге/контейнере CI/CD.

---

## Структура проекта

```
.
├─ docker-compose.yml
├─ .env.example
└─ src/
   └─ MyApp.Api/
      ├─ MyApp.Api.csproj
      ├─ Program.cs
      ├─ appsettings.json
      ├─ appsettings.Development.json
      ├─ Dockerfile
      ├─ Data/
      │  └─ AppDbContext.cs
      ├─ Models/
      │  └─ Post.cs
      ├─ DTOs/
      │  ├─ AuthDtos.cs
      │  └─ PostDtos.cs
      ├─ Services/
      │  └─ JwtTokenService.cs
      └─ Controllers/
         ├─ AuthController.cs
         └─ PostsController.cs
```

---

## Расширение API: как добавить новую сущность

**Примерный алгоритм:**

1. Создай модель `Models/Course.cs`
2. Добавь `DbSet<Course>` в `AppDbContext`
3. (Если используешь миграции) сгенерируй и примени миграцию
4. Создай DTO в `DTOs/`
5. Создай контроллер `Controllers/CoursesController.cs` (GET/POST/PUT/DELETE)
6. При необходимости повесь `[Authorize]` или `[Authorize(Roles = "admin")]`
7. Пересобери и перезапусти контейнеры

---

## Подсказки по продакшену

* Вынесите API за реверс‑прокси (Nginx/Traefik), включите HTTPS
* Уточните CORS (строгий список доменов)
* Ужесточите CSP (Content‑Security‑Policy) заголовок
* Поднимите rate limit и логирование в соответствии с нагрузкой
* Не публикуйте phpMyAdmin наружу; доступ — только по VPN/SSH
* Храните секреты в менеджере секретов (Vault/Parameter Store)
* Делайте бэкапы БД (dump/пер‑снапшоты volume)

---

## Устранение неполадок

**Swagger открывается, но логин даёт 401**

* Проверь `JWT_SECRET`, `JWT_ISSUER`, `JWT_AUDIENCE` — при выпуске и проверке должны совпадать
* Убедись, что отправляешь `Authorization: Bearer <token>`

**Админ не создался**

* Учётка из `ADMIN_*` создаётся только при первом старте пустой БД
* Если БД уже существовала — зарегистрируй админа через Swagger **или** удали volume `mysql_data` и подними заново

**Порт занят**

* Измени хост‑порты в `docker-compose.yml` (например, `8080:8080` → `8088:8080`)

**Изменил .env, но ничего не меняется**

* Делай `docker compose down` и затем `docker compose up --build -d`

**MySQL не принимает соединение**

* Проверь `docker compose ps` и логи `docker compose logs mysql`
* Жди, пока MySQL станет «healthy»; API стартует после healthcheck

---

## Полезные команды Docker

```bash
# Старт/стоп
docker compose up --build -d
docker compose down

docker compose ps

# Логи
docker compose logs -f api

# Пересборка только API
docker compose build api && docker compose up -d api

# Полный сброс БД (удалит данные!)
docker compose down -v

# Доступ в контейнер API
docker compose exec api sh

# Дамп БД (пример)
docker compose exec mysql mysqldump -umyapp -pdevpass123 myapp > backup.sql

# Восстановление БД (пример)
cat backup.sql | docker compose exec -T mysql mysql -umyapp -pdevpass123 myapp
```

---

**Готово!** Если всё по инструкции — API доступен на `http://localhost:8080/swagger`, БД — на `http://localhost:8082`.