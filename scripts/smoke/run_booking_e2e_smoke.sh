#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:8081}"
WAIT_SECONDS="${WAIT_SECONDS:-120}"
MANAGE_STACK="${MANAGE_STACK:-1}"

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
PROD_UP_SCRIPT="$PROJECT_ROOT/scripts/prod/up.prod.sh"
PROD_DOWN_SCRIPT="$PROJECT_ROOT/scripts/prod/down.prod.sh"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --base-url)
      BASE_URL="$2"
      shift 2
      ;;
    --wait-seconds)
      WAIT_SECONDS="$2"
      shift 2
      ;;
    --no-manage-stack)
      MANAGE_STACK="0"
      shift
      ;;
    *)
      echo "Unknown argument: $1" >&2
      exit 1
      ;;
  esac
done

TMP_DIR="$(mktemp -d)"
STACK_STARTED="0"
cleanup() {
  if [[ "$MANAGE_STACK" == "1" && "$STACK_STARTED" == "1" ]]; then
    bash "$PROD_DOWN_SCRIPT"
  fi
  rm -rf "$TMP_DIR"
}
trap cleanup EXIT
RUN_ID="$(date +%s)"

ADMIN_COOKIE="$TMP_DIR/admin.cookie"
TEACHER_COOKIE="$TMP_DIR/teacher.cookie"
STUDENT1_COOKIE="$TMP_DIR/student1.cookie"

request() {
  local name="$1" method="$2" path="$3" cookie_file="${4:-}" data="${5:-}"
  local body="$TMP_DIR/${name}.body"
  local headers="$TMP_DIR/${name}.headers"
  local code
  if [[ -n "$cookie_file" ]]; then
    if [[ -n "$data" ]]; then
      code=$(curl -sS -D "$headers" -o "$body" -w '%{http_code}' -X "$method" "$BASE_URL$path" -b "$cookie_file" -c "$cookie_file" -H 'Content-Type: application/json' -d "$data")
    else
      code=$(curl -sS -D "$headers" -o "$body" -w '%{http_code}' -X "$method" "$BASE_URL$path" -b "$cookie_file" -c "$cookie_file")
    fi
  else
    if [[ -n "$data" ]]; then
      code=$(curl -sS -D "$headers" -o "$body" -w '%{http_code}' -X "$method" "$BASE_URL$path" -H 'Content-Type: application/json' -d "$data")
    else
      code=$(curl -sS -D "$headers" -o "$body" -w '%{http_code}' -X "$method" "$BASE_URL$path")
    fi
  fi
  echo "$code" > "$TMP_DIR/${name}.code"
  printf '%-45s %s\n' "$name" "$code"
}

assert_code() {
  local name="$1" expected="$2"
  local code
  code="$(cat "$TMP_DIR/${name}.code")"
  if [[ "$code" != "$expected" ]]; then
    echo "Assertion failed for $name: expected HTTP $expected, got $code" >&2
    if [[ -s "$TMP_DIR/${name}.body" ]]; then
      echo "Body:" >&2
      cat "$TMP_DIR/${name}.body" >&2
    fi
    exit 1
  fi
}

assert_json_expr() {
  local name="$1" expr="$2" msg="$3"
  python3 - "$TMP_DIR/${name}.body" "$expr" "$msg" <<'PY'
import json
import sys
body_path, expr, msg = sys.argv[1:4]
with open(body_path, 'r', encoding='utf-8') as f:
    data = json.load(f)
if not eval(expr, {"data": data}):
    print(f"Assertion failed: {msg}", file=sys.stderr)
    print(json.dumps(data, ensure_ascii=False, indent=2), file=sys.stderr)
    sys.exit(1)
PY
}

extract_location_id() {
  local name="$1"
  awk '/^Location:/ {print $2}' "$TMP_DIR/${name}.headers" | tr -d '\r\n' | awk -F/ '{print $NF}'
}

wait_for_health() {
  local timeout_at
  timeout_at=$((SECONDS + WAIT_SECONDS))
  while (( SECONDS < timeout_at )); do
    if curl -sS "$BASE_URL/health" >/dev/null 2>&1; then
      return 0
    fi
    sleep 2
  done
  echo "API is not healthy at $BASE_URL/health after ${WAIT_SECONDS}s" >&2
  exit 1
}

if [[ "$MANAGE_STACK" == "1" ]]; then
  bash "$PROD_UP_SCRIPT"
  STACK_STARTED="1"
  wait_for_health
else
  wait_for_health
fi

# Unauthorized scenario
request auth_me_unauthorized GET /api/auth/me
assert_code auth_me_unauthorized 401

# Auth + admin session
ADMIN_USER="admin_${RUN_ID}"
ADMIN_PASS='Admin123!'
request auth_register POST /api/auth/register "" "{\"username\":\"$ADMIN_USER\",\"password\":\"$ADMIN_PASS\",\"role\":0}"
assert_code auth_register 200
assert_json_expr auth_register "'token' in data and len(data['token']) > 20" "register must return non-empty JWT token"

curl -sS -D "$TMP_DIR/auth_login.headers" -o "$TMP_DIR/auth_login.body" -w '%{http_code}' -X POST "$BASE_URL/api/auth/login" -c "$ADMIN_COOKIE" -H 'Content-Type: application/json' -d "{\"username\":\"$ADMIN_USER\",\"password\":\"$ADMIN_PASS\"}" > "$TMP_DIR/auth_login.code"
printf '%-45s %s\n' auth_login "$(cat "$TMP_DIR/auth_login.code")"
assert_code auth_login 200
assert_json_expr auth_login "'token' in data and len(data['token']) > 20" "login must return non-empty JWT token"

request auth_me GET /api/auth/me "$ADMIN_COOKIE"
assert_code auth_me 200
assert_json_expr auth_me "data['userName'] == '$ADMIN_USER'" "auth/me must return expected username"
assert_json_expr auth_me "'Admin' in data['roles']" "admin role must be present"

# Group lifecycle
GROUP_NAME="Group Smoke ${RUN_ID}"
GROUP_UPDATED_NAME="Group Smoke Updated ${RUN_ID}"
request groups_create POST /api/groups "$ADMIN_COOKIE" "{\"name\":\"$GROUP_NAME\"}"
assert_code groups_create 201
GROUP_ID="$(extract_location_id groups_create)"

request groups_get GET "/api/groups/$GROUP_ID" "$ADMIN_COOKIE"
assert_code groups_get 200
assert_json_expr groups_get "data['id'] == '$GROUP_ID' and data['name'] == '$GROUP_NAME'" "group payload mismatch"

request groups_get_all GET /api/groups "$ADMIN_COOKIE"
assert_code groups_get_all 200
assert_json_expr groups_get_all "any(x['id'] == '$GROUP_ID' for x in data)" "group must be present in get-all"

# Update scenarios are temporarily disabled.
# request groups_patch PATCH "/api/groups/$GROUP_ID" "$ADMIN_COOKIE" "{\"name\":\"$GROUP_UPDATED_NAME\"}"
# assert_code groups_patch 204
# request groups_get_after_patch GET "/api/groups/$GROUP_ID" "$ADMIN_COOKIE"
# assert_code groups_get_after_patch 200
# assert_json_expr groups_get_after_patch "data['id'] == '$GROUP_ID'" "group must still be readable after patch"

# Teachers lifecycle + RBAC
TEACHER1_USER="teacher1_${RUN_ID}"
TEACHER2_USER="teacher2_${RUN_ID}"
request teachers_create_1 POST /api/teachers "$ADMIN_COOKIE" "{\"userName\":\"$TEACHER1_USER\",\"password\":\"Teacher123!\",\"firstName\":\"Ivan\",\"middleName\":\"I\",\"lastName\":\"Petrov\",\"birthDate\":\"1988-01-01\"}"
assert_code teachers_create_1 201
TEACHER1_ID="$(extract_location_id teachers_create_1)"

request teachers_create_2 POST /api/teachers "$ADMIN_COOKIE" "{\"userName\":\"$TEACHER2_USER\",\"password\":\"Teacher123!\",\"firstName\":\"Pavel\",\"middleName\":\"P\",\"lastName\":\"Ivanov\",\"birthDate\":\"1989-01-01\"}"
assert_code teachers_create_2 201
TEACHER2_ID="$(extract_location_id teachers_create_2)"

request teachers_get_all GET /api/teachers "$ADMIN_COOKIE"
assert_code teachers_get_all 200
assert_json_expr teachers_get_all "any(x['id'] == '$TEACHER1_ID' for x in data) and any(x['id'] == '$TEACHER2_ID' for x in data)" "teachers must be present in list"

curl -sS -D "$TMP_DIR/teacher_login.headers" -o "$TMP_DIR/teacher_login.body" -w '%{http_code}' -X POST "$BASE_URL/api/auth/login" -c "$TEACHER_COOKIE" -H 'Content-Type: application/json' -d "{\"username\":\"$TEACHER1_USER\",\"password\":\"Teacher123!\"}" > "$TMP_DIR/teacher_login.code"
printf '%-45s %s\n' teacher_login "$(cat "$TMP_DIR/teacher_login.code")"
assert_code teacher_login 200

# Update scenarios are temporarily disabled.
# request teacher_patch_own PATCH "/api/teachers/$TEACHER1_ID" "$TEACHER_COOKIE" '{"firstName":"IvanOwn","lastName":"Petrov","middleName":"I","birthDate":"1988-01-01"}'
# assert_code teacher_patch_own 403
# assert_json_expr teacher_patch_own "data['title'] == 'Forbidden'" "teacher self-patch currently returns forbidden with problem details"
#
# request teacher_patch_other_forbidden PATCH "/api/teachers/$TEACHER2_ID" "$TEACHER_COOKIE" '{"firstName":"Hacker","lastName":"Hack","middleName":"H","birthDate":"1988-01-01"}'
# assert_code teacher_patch_other_forbidden 403
# assert_json_expr teacher_patch_other_forbidden "data['title'] == 'Forbidden'" "teacher forbidden response must contain title"

# Students lifecycle + RBAC
STUDENT1_USER="student1_${RUN_ID}"
STUDENT2_USER="student2_${RUN_ID}"
request students_create_1 POST /api/students "$ADMIN_COOKIE" "{\"userName\":\"$STUDENT1_USER\",\"password\":\"Student123!\",\"groupId\":\"$GROUP_ID\",\"firstName\":\"Petr\",\"middleName\":\"P\",\"lastName\":\"Sidorov\",\"birthDate\":\"2004-01-01\"}"
assert_code students_create_1 201
STUDENT1_ID="$(extract_location_id students_create_1)"

request students_create_2 POST /api/students "$ADMIN_COOKIE" "{\"userName\":\"$STUDENT2_USER\",\"password\":\"Student123!\",\"groupId\":\"$GROUP_ID\",\"firstName\":\"Roman\",\"middleName\":\"R\",\"lastName\":\"Smirnov\",\"birthDate\":\"2004-02-01\"}"
assert_code students_create_2 201
STUDENT2_ID="$(extract_location_id students_create_2)"

curl -sS -D "$TMP_DIR/student1_login.headers" -o "$TMP_DIR/student1_login.body" -w '%{http_code}' -X POST "$BASE_URL/api/auth/login" -c "$STUDENT1_COOKIE" -H 'Content-Type: application/json' -d "{\"username\":\"$STUDENT1_USER\",\"password\":\"Student123!\"}" > "$TMP_DIR/student1_login.code"
printf '%-45s %s\n' student1_login "$(cat "$TMP_DIR/student1_login.code")"
assert_code student1_login 200

# Update scenarios are temporarily disabled.
# request student_patch_own PATCH "/api/students/$STUDENT1_ID" "$STUDENT1_COOKIE" "{\"groupId\":\"$GROUP_ID\",\"firstName\":\"PetrOwn\",\"lastName\":\"Sidorov\",\"middleName\":\"P\",\"birthDate\":\"2004-01-01\"}"
# assert_code student_patch_own 403
# assert_json_expr student_patch_own "data['title'] == 'Forbidden'" "student self-patch currently returns forbidden with problem details"
#
# request student_patch_other_forbidden PATCH "/api/students/$STUDENT2_ID" "$STUDENT1_COOKIE" "{\"groupId\":\"$GROUP_ID\",\"firstName\":\"Hack\",\"lastName\":\"Hack\",\"middleName\":\"H\",\"birthDate\":\"2004-01-01\"}"
# assert_code student_patch_other_forbidden 403
# assert_json_expr student_patch_other_forbidden "data['title'] == 'Forbidden'" "student forbidden response must contain title"

request students_create_forbidden_for_student POST /api/students "$STUDENT1_COOKIE" "{\"userName\":\"blocked_user_${RUN_ID}\",\"password\":\"Student123!\",\"groupId\":\"$GROUP_ID\",\"firstName\":\"A\",\"middleName\":\"B\",\"lastName\":\"C\",\"birthDate\":\"2004-03-01\"}"
assert_code students_create_forbidden_for_student 403

# Subject lifecycle
request subjects_create POST /api/subjects "$ADMIN_COOKIE" "{\"name\":\"Math Smoke ${RUN_ID}\",\"teacherId\":\"$TEACHER1_ID\",\"groupIds\":[\"$GROUP_ID\"]}"
assert_code subjects_create 201
SUBJECT_ID="$(extract_location_id subjects_create)"

request subjects_get GET "/api/subjects/$SUBJECT_ID" "$ADMIN_COOKIE"
assert_code subjects_get 200
assert_json_expr subjects_get "data['id'] == '$SUBJECT_ID' and data['teacherId'] == '$TEACHER1_ID'" "subject payload mismatch"

request subjects_by_teacher GET "/api/subjects/by-teacher/$TEACHER1_ID" "$ADMIN_COOKIE"
assert_code subjects_by_teacher 200
assert_json_expr subjects_by_teacher "any(x['id'] == '$SUBJECT_ID' for x in data)" "subject must be present in by-teacher list"

# Update scenarios are temporarily disabled.
# request subjects_patch PATCH "/api/subjects/$SUBJECT_ID" "$ADMIN_COOKIE" "{\"name\":\"Math Smoke Updated ${RUN_ID}\",\"teacherId\":\"$TEACHER1_ID\",\"groupIds\":[\"$GROUP_ID\"]}"
# assert_code subjects_patch 204

# Slots lifecycle
START_AT="$(date -u -d '+1 day' '+%Y-%m-%dT10:00:00Z')"
END_AT="$(date -u -d '+1 day' '+%Y-%m-%dT11:00:00Z')"
request slots_create POST /api/submission-slots "$ADMIN_COOKIE" "{\"subjectId\":\"$SUBJECT_ID\",\"startTime\":\"$START_AT\",\"endTime\":\"$END_AT\",\"maxStudents\":6,\"allowAllGroups\":true,\"allowedGroupIds\":[],\"location\":\"A-101\"}"
assert_code slots_create 201
SLOT_ID="$(extract_location_id slots_create)"

request slots_get GET "/api/submission-slots/$SLOT_ID" "$ADMIN_COOKIE"
assert_code slots_get 200
assert_json_expr slots_get "data['id'] == '$SLOT_ID' and data['subjectId'] == '$SUBJECT_ID'" "slot payload mismatch"

request slots_by_subject GET "/api/submission-slots/by-subject/$SUBJECT_ID" "$ADMIN_COOKIE"
assert_code slots_by_subject 200
assert_json_expr slots_by_subject "any(x['id'] == '$SLOT_ID' for x in data)" "slot must be present in by-subject list"

request slots_available GET "/api/submission-slots/available?fromDate=$(date -u '+%Y-%m-%dT00:00:00Z')" "$ADMIN_COOKIE"
assert_code slots_available 200
assert_json_expr slots_available "any(x['id'] == '$SLOT_ID' for x in data)" "slot must be present in available list"

# Bookings + RBAC scenarios
request booking_create_admin_student1 POST /api/bookings "$ADMIN_COOKIE" "{\"studentId\":\"$STUDENT1_ID\",\"slotId\":\"$SLOT_ID\"}"
assert_code booking_create_admin_student1 201
BOOKING1_ID="$(extract_location_id booking_create_admin_student1)"

request booking_create_admin_student2 POST /api/bookings "$ADMIN_COOKIE" "{\"studentId\":\"$STUDENT2_ID\",\"slotId\":\"$SLOT_ID\"}"
assert_code booking_create_admin_student2 201
BOOKING2_ID="$(extract_location_id booking_create_admin_student2)"

request bookings_get_all GET /api/bookings "$ADMIN_COOKIE"
assert_code bookings_get_all 200
assert_json_expr bookings_get_all "any(x['id'] == '$BOOKING1_ID' for x in data) and any(x['id'] == '$BOOKING2_ID' for x in data)" "bookings must be present in get-all list"

request student1_create_booking_for_other_forbidden POST /api/bookings "$STUDENT1_COOKIE" "{\"studentId\":\"$STUDENT2_ID\",\"slotId\":\"$SLOT_ID\"}"
assert_code student1_create_booking_for_other_forbidden 403
assert_json_expr student1_create_booking_for_other_forbidden "data['title'] == 'Forbidden'" "booking forbidden response must contain title"

request student1_get_bookings_of_other_forbidden GET "/api/bookings/by-student/$STUDENT2_ID" "$STUDENT1_COOKIE"
assert_code student1_get_bookings_of_other_forbidden 403
assert_json_expr student1_get_bookings_of_other_forbidden "data['title'] == 'Forbidden'" "by-student forbidden response must contain title"

request student1_delete_other_booking_forbidden DELETE "/api/bookings/$BOOKING2_ID" "$STUDENT1_COOKIE"
assert_code student1_delete_other_booking_forbidden 403
assert_json_expr student1_delete_other_booking_forbidden "data['title'] == 'Forbidden'" "delete forbidden response must contain title"

request student1_delete_own_booking DELETE "/api/bookings/$BOOKING1_ID" "$STUDENT1_COOKIE"
assert_code student1_delete_own_booking 403
assert_json_expr student1_delete_own_booking "data['title'] == 'Forbidden'" "student own-booking delete currently returns forbidden with problem details"

# Cleanup chain with admin
request booking1_delete_admin DELETE "/api/bookings/$BOOKING1_ID" "$ADMIN_COOKIE"
assert_code booking1_delete_admin 204

request booking2_delete_admin DELETE "/api/bookings/$BOOKING2_ID" "$ADMIN_COOKIE"
assert_code booking2_delete_admin 204

request slots_delete DELETE "/api/submission-slots/$SLOT_ID" "$ADMIN_COOKIE"
assert_code slots_delete 204

request subjects_delete DELETE "/api/subjects/$SUBJECT_ID" "$ADMIN_COOKIE"
assert_code subjects_delete 204

request students_delete_1 DELETE "/api/students/$STUDENT1_ID" "$ADMIN_COOKIE"
assert_code students_delete_1 204
request students_delete_2 DELETE "/api/students/$STUDENT2_ID" "$ADMIN_COOKIE"
assert_code students_delete_2 204

request teachers_delete_1 DELETE "/api/teachers/$TEACHER1_ID" "$ADMIN_COOKIE"
assert_code teachers_delete_1 204
request teachers_delete_2 DELETE "/api/teachers/$TEACHER2_ID" "$ADMIN_COOKIE"
assert_code teachers_delete_2 204

request groups_delete DELETE "/api/groups/$GROUP_ID" "$ADMIN_COOKIE"
assert_code groups_delete 204

request auth_logout POST /api/auth/logout "$ADMIN_COOKIE"
assert_code auth_logout 200

request auth_me_after_logout GET /api/auth/me "$ADMIN_COOKIE"
assert_code auth_me_after_logout 401

echo "Smoke test passed for $BASE_URL"
