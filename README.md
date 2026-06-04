# EFlow

<p align="center">
	<img src=".github/images/logo.svg" alt="Logo" style="width: 20%">
</p>

EFlow is a booking and notification system for education processes.

## Local Development

### 1. Run debug infrastructure PostgreSQL, Kafka, and MailDev in Docker.

```bash
./scripts/debug/up.debug.sh
```

This uses `docker/debug.env` and starts:

- PostgreSQL on `localhost:5433`
- Kafka on `localhost:19092`
- MailDev SMTP on `localhost:1025`
- MailDev Web UI on `localhost:1080`

To stop the infrastructure:

```bash
./scripts/debug/down.debug.sh
```

### 2. Run Backend in development mode.

```bash
dotnet run --project EFlow.Booking/Presentation/EFlow.Booking.WebApi/EFlow.Booking.WebApi.csproj
dotnet run --project EFlow.Notifications/Presentation/EFlow.Notifications.WebApi/EFlow.Notifications.WebApi.csproj
```

### 3. Run Angular UI in development mode.

```bash
cd UI
npm i
npm run start
```

Default development admin credentials (for UI):

```text
Username: admin
Password: admin123
```

## Production Docker Setup

Production Compose builds and runs PostgreSQL, Kafka, Booking API, Notifications API, Angular UI, and Caddy.


### Start production stack

```bash
./scripts/prod/up.prod.sh
```

The first run creates `docker/prod.env` from `docker/template.prod.env` if it does not exist, asks you to fill it, and exits. Re-run the script after filling the values.

Edit `docker/prod.env` and fill all required values.

Minimum required values:

```env
POSTGRES_DB=eflow
POSTGRES_USER=postgres
POSTGRES_PASSWORD=change_me
POSTGRES_PORT=5433

KAFKA_PORT=19092

BOOKING_API_PORT=8081
NOTIFICATIONS_API_PORT=8082
CORS_ALLOWED_ORIGIN_0=https://your-domain.example

JWT_KEY=replace_with_a_long_random_secret_at_least_32_chars
JWT_ISSUER=https://your-domain.example
JWT_AUDIENCE=https://your-domain.example
JWT_EXPIRE_MINUTES=1440

ADMIN_USERNAME=admin
ADMIN_PASSWORD=change_me
ADMIN_EMAIL=admin@example.com

SMTP_HOST=smtp.example.com
SMTP_PORT=587
SMTP_USERNAME=smtp_user
SMTP_PASSWORD=smtp_password
SMTP_FROM_EMAIL=notifications@example.com
SMTP_FROM_NAME=EFlow
SMTP_SECURE_SOCKET_OPTIONS=StartTls
```

### Stop production stack

```bash
./scripts/prod/down.prod.sh
```

## API Client Generation

The UI contains a generated API contract file at `UI/src/app/api/contracts.ts`.

To regenerate it:

```bash
cd UI
npm run api:generate
```

The script starts Booking API in the `OpenApiGenerator` environment on `127.0.0.1:5117`, downloads `/openapi/v1.json`, and writes:

- `UI/src/app/api/openapi.json`
- `UI/src/app/api/contracts.ts`

## Testing

Run all .NET tests:

```bash
dotnet test EFlow.slnx
```

Run UI tests:

```bash
cd UI
npm test
```

## License

This project is licensed under the MIT License. See `LICENSE` for details.
