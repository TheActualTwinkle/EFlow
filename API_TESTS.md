# EFlow.Booking API Tests

## Local run

```bash
./scripts/test/run_booking_api_tests.sh
```

## What is covered

- Authentication flow (`register`, `login`, `me`, `logout`)
- CRUD paths for `groups`, `teachers`, `students`, `subjects`, `submission-slots`, `bookings`
- Authorization scenarios (403/401) for teacher/student restricted actions
- JSON body validation (critical fields and list presence)
