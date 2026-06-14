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

## Build from source

```bash
./scripts/prod/build.prod.sh 1.0.0
```

This will build all required stuff and save it to `out/eflow-1.0.0` folder in repo root.

### Start production stack

```bash
./out/eflow-1.0.0/up.sh
```

If you didn`t have `prod.env` in `/docker` it`ll not be copied to the out directory
So the first run will force you to fill in the `.env` file with your production configuration.

### Stop production stack

```bash
./out/eflow-1.0.0/down.sh
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
