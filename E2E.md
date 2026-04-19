# EFlow.Booking Smoke API Tests

## Local run

```bash
./scripts/smoke/run_booking_e2e_smoke.sh
```

## What is covered

- Authentication flow (`register`, `login`, `me`, `logout`)
- CRUD paths for `groups`, `teachers`, `students`, `subjects`, `submission-slots`, `bookings`
- Authorization scenarios (403/401) for teacher/student restricted actions
- JSON body validation (critical fields and list presence)
