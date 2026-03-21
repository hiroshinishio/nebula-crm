#!/usr/bin/env bash
# F0014: Automated smoke test for Nebula CRM API.
# Verifies auth, core CRUD endpoints, and timeline event recording
# against a running docker-compose stack.
#
# Usage:
#   ./scripts/smoke-test.sh                  # Run against already-running stack
#   ./scripts/smoke-test.sh --reset          # Tear down, rebuild, then test
#   ./scripts/smoke-test.sh --user lisa.wong  # Test as specific user (default: lisa.wong)
#
# Prerequisites:
#   - docker compose services running (or use --reset)
#   - curl, python3 available on PATH
#
# Exit codes:
#   0 — all tests passed
#   1 — one or more tests failed
#   2 — setup/infrastructure failure

set -euo pipefail

# ── Configuration ───────────────────────────────────────────────────────
API_BASE="${API_BASE:-http://localhost:8080}"
AUTHENTIK_BASE="${AUTHENTIK_BASE:-http://localhost:9000}"
TOKEN_ENDPOINT="${AUTHENTIK_BASE}/application/o/token/"
CLIENT_ID="${CLIENT_ID:-nebula}"
SCOPES="openid profile email nebula_roles broker_tenant_id"
# All dev users share this app-password token key (seeded by blueprint)
APP_PASSWORD="${APP_PASSWORD:-nebula-dev-token}"
TEST_USER="${TEST_USER:-lisa.wong}"
COMPOSE_PROJECT_DIR="${COMPOSE_PROJECT_DIR:-$(cd "$(dirname "$0")/.." && pwd)}"

# ── CLI arg parsing ─────────────────────────────────────────────────────
RESET=false
while [[ $# -gt 0 ]]; do
  case "$1" in
    --reset)      RESET=true; shift ;;
    --user)       TEST_USER="$2"; shift 2 ;;
    --api)        API_BASE="$2"; shift 2 ;;
    --help|-h)
      sed -n '2,/^$/p' "$0" | sed 's/^# \?//'
      exit 0 ;;
    *) echo "Unknown option: $1"; exit 2 ;;
  esac
done

# ── Counters ────────────────────────────────────────────────────────────
PASSED=0
FAILED=0
TOTAL=0

pass() { PASSED=$((PASSED + 1)); TOTAL=$((TOTAL + 1)); echo "  PASS: $1"; }
fail() { FAILED=$((FAILED + 1)); TOTAL=$((TOTAL + 1)); echo "  FAIL: $1 — $2"; }

# ── Reset (optional) ───────────────────────────────────────────────────
if [[ "$RESET" == "true" ]]; then
  echo "==> Tearing down stack (volumes included)..."
  cd "$COMPOSE_PROJECT_DIR"
  docker compose down -v 2>&1 | tail -3
  echo "==> Rebuilding and starting..."
  docker compose up -d --build 2>&1 | tail -5
  echo ""
fi

# ── Wait for services ──────────────────────────────────────────────────
echo "==> Waiting for services..."

wait_for_url() {
  local url="$1" label="$2" max_attempts="${3:-30}"
  for i in $(seq 1 "$max_attempts"); do
    if curl -sf -o /dev/null "$url" 2>/dev/null; then
      echo "  $label ready (attempt $i)"
      return 0
    fi
    sleep 2
  done
  echo "  TIMEOUT: $label not ready after $((max_attempts * 2))s"
  return 1
}

wait_for_url "${API_BASE}/healthz" "API" 45 || exit 2
wait_for_url "${AUTHENTIK_BASE}/-/health/live/" "authentik" 30 || exit 2
echo ""

# ── Acquire JWT ─────────────────────────────────────────────────────────
echo "==> Acquiring JWT for ${TEST_USER}..."
TOKEN_RESP=$(curl -sf -X POST "$TOKEN_ENDPOINT" \
  -d "grant_type=password&client_id=${CLIENT_ID}&username=${TEST_USER}&password=${APP_PASSWORD}&scope=${SCOPES}" 2>&1) || {
  echo "  FAIL: Could not acquire token. Response: $TOKEN_RESP"
  echo ""
  echo "  Troubleshooting:"
  echo "    1. Is authentik healthy?  curl ${AUTHENTIK_BASE}/-/health/live/"
  echo "    2. Was the blueprint applied? Check: docker compose logs authentik-worker | grep -i blueprint"
  echo "    3. Does the user exist?   docker compose exec authentik-server ak shell -c \"from authentik.core.models import User; print(User.objects.filter(username='${TEST_USER}').exists())\""
  echo "    4. Does the app-password token exist?  Check blueprint for authentik_core.token entries"
  exit 2
}

TOKEN=$(echo "$TOKEN_RESP" | python3 -c "import sys,json; print(json.load(sys.stdin)['access_token'])")
echo "  Token acquired (${#TOKEN} chars)"

# Verify expected claims
CLAIMS=$(python3 -c "
import json, base64
parts = '$TOKEN'.split('.')
payload = parts[1] + '=' * (4 - len(parts[1]) % 4)
d = json.loads(base64.urlsafe_b64decode(payload))
print(json.dumps({'sub': d.get('sub'), 'aud': d.get('aud'), 'nebula_roles': d.get('nebula_roles', [])}))")
echo "  Claims: $CLAIMS"
echo ""

AUTH="Authorization: Bearer $TOKEN"

# ── Helper: HTTP request ────────────────────────────────────────────────
# Returns: body\nhttp_code
http() {
  local method="$1" url="$2"
  shift 2
  curl -s -w "\n%{http_code}" -X "$method" "${API_BASE}${url}" \
    -H "$AUTH" -H "Content-Type: application/json" "$@"
}

http_code() { echo "$1" | tail -1; }
http_body() { echo "$1" | sed '$d'; }
json_field() { echo "$1" | python3 -c "import sys,json; print(json.load(sys.stdin).get('$2',''))" 2>/dev/null; }

# ── Get internal UserId (triggers UserProfile upsert) ──────────────────
echo "==> Resolving internal UserId..."
RESP=$(http GET "/my/tasks?limit=1")
CODE=$(http_code "$RESP")
BODY=$(http_body "$RESP")

if [[ "$CODE" != "200" ]]; then
  echo "  FAIL: GET /my/tasks returned $CODE — cannot resolve UserId"
  echo "  Body: $BODY"
  exit 2
fi

# Query DB for the internal UserId
USER_ID=$(docker compose exec -T db psql -U postgres -d nebula -t -A -c \
  "SELECT \"Id\" FROM \"UserProfiles\" WHERE \"IdpSubject\" = '${TEST_USER}' LIMIT 1;" 2>/dev/null | tr -d '[:space:]')

if [[ -z "$USER_ID" ]]; then
  echo "  FAIL: UserProfile not found for ${TEST_USER}"
  exit 2
fi
echo "  UserId: $USER_ID"
echo ""

# ════════════════════════════════════════════════════════════════════════
#  SMOKE TESTS
# ════════════════════════════════════════════════════════════════════════
echo "==> Running smoke tests..."
echo ""

# ── 1. GET /my/tasks ────────────────────────────────────────────────────
echo "[1/9] GET /my/tasks"
RESP=$(http GET "/my/tasks?limit=5")
CODE=$(http_code "$RESP"); BODY=$(http_body "$RESP")
if [[ "$CODE" == "200" ]]; then
  TOTAL_COUNT=$(json_field "$BODY" "totalCount")
  pass "200 OK (totalCount=$TOTAL_COUNT)"
else
  fail "Expected 200, got $CODE" "$BODY"
fi

# ── 2. POST /tasks (create) ────────────────────────────────────────────
echo "[2/9] POST /tasks (create)"
RESP=$(http POST "/tasks" -d "{\"title\":\"Smoke test task\",\"description\":\"Automated verification\",\"priority\":\"High\",\"dueDate\":\"2026-04-01T00:00:00Z\",\"assignedToUserId\":\"$USER_ID\"}")
CODE=$(http_code "$RESP"); BODY=$(http_body "$RESP")
if [[ "$CODE" == "201" ]]; then
  TASK_ID=$(json_field "$BODY" "id")
  TASK_STATUS=$(json_field "$BODY" "status")
  pass "201 Created (id=$TASK_ID, status=$TASK_STATUS)"
else
  fail "Expected 201, got $CODE" "$BODY"
  echo "  ABORT: Cannot continue without created task"
  exit 1
fi

# ── 3. GET /tasks/{id} ─────────────────────────────────────────────────
echo "[3/9] GET /tasks/{id}"
RESP=$(http GET "/tasks/$TASK_ID")
CODE=$(http_code "$RESP"); BODY=$(http_body "$RESP")
if [[ "$CODE" == "200" ]]; then
  TITLE=$(json_field "$BODY" "title")
  pass "200 OK (title=$TITLE)"
else
  fail "Expected 200, got $CODE" "$BODY"
fi

# ── 4. PUT /tasks/{id} — Open → InProgress ─────────────────────────────
echo "[4/9] PUT /tasks/{id} (Open -> InProgress)"
RESP=$(http PUT "/tasks/$TASK_ID" -d '{"status":"InProgress"}')
CODE=$(http_code "$RESP"); BODY=$(http_body "$RESP")
if [[ "$CODE" == "200" ]]; then
  NEW_STATUS=$(json_field "$BODY" "status")
  [[ "$NEW_STATUS" == "InProgress" ]] && pass "200 OK (status=InProgress)" || fail "Status mismatch" "expected InProgress, got $NEW_STATUS"
else
  fail "Expected 200, got $CODE" "$BODY"
fi

# ── 5. PUT /tasks/{id} — InProgress → Done ─────────────────────────────
echo "[5/9] PUT /tasks/{id} (InProgress -> Done)"
RESP=$(http PUT "/tasks/$TASK_ID" -d '{"status":"Done"}')
CODE=$(http_code "$RESP"); BODY=$(http_body "$RESP")
if [[ "$CODE" == "200" ]]; then
  COMPLETED_AT=$(json_field "$BODY" "completedAt")
  [[ -n "$COMPLETED_AT" ]] && pass "200 OK (completedAt=$COMPLETED_AT)" || fail "completedAt missing" "$BODY"
else
  fail "Expected 200, got $CODE" "$BODY"
fi

# ── 6. PUT — invalid transition (Open → Done = 409) ────────────────────
echo "[6/9] PUT — invalid transition (Open -> Done)"
# Create a fresh task for this test
RESP2=$(http POST "/tasks" -d "{\"title\":\"Transition guard test\",\"assignedToUserId\":\"$USER_ID\"}")
TASK2_ID=$(json_field "$(http_body "$RESP2")" "id")
RESP=$(http PUT "/tasks/$TASK2_ID" -d '{"status":"Done"}')
CODE=$(http_code "$RESP"); BODY=$(http_body "$RESP")
if [[ "$CODE" == "409" ]]; then
  ERR_CODE=$(json_field "$BODY" "code")
  pass "409 Conflict (code=$ERR_CODE)"
else
  fail "Expected 409, got $CODE" "$BODY"
fi

# ── 7. DELETE /tasks/{id} ──────────────────────────────────────────────
echo "[7/9] DELETE /tasks/{id}"
RESP=$(http DELETE "/tasks/$TASK_ID")
CODE=$(http_code "$RESP")
if [[ "$CODE" == "204" ]]; then
  pass "204 No Content"
else
  fail "Expected 204, got $CODE" "$(http_body "$RESP")"
fi

# ── 8. GET deleted task (expect 404) ───────────────────────────────────
echo "[8/9] GET deleted task (expect 404)"
RESP=$(http GET "/tasks/$TASK_ID")
CODE=$(http_code "$RESP")
if [[ "$CODE" == "404" ]]; then
  pass "404 Not Found (soft delete confirmed)"
else
  fail "Expected 404, got $CODE" "$(http_body "$RESP")"
fi

# ── 9. Timeline events verification ───────────────────────────────────
echo "[9/9] Timeline events for smoke test task"
EVENTS=$(docker compose exec -T db psql -U postgres -d nebula -t -A -c \
  "SELECT \"EventType\" FROM \"ActivityTimelineEvents\" WHERE \"EntityId\" = '$TASK_ID' ORDER BY \"OccurredAt\";" 2>/dev/null | tr '\n' ',')
EXPECTED="TaskCreated,TaskUpdated,TaskCompleted,TaskDeleted,"
if [[ "$EVENTS" == "$EXPECTED" ]]; then
  pass "4 timeline events: TaskCreated,TaskUpdated,TaskCompleted,TaskDeleted"
else
  fail "Expected $EXPECTED" "got $EVENTS"
fi

# ── Cleanup ─────────────────────────────────────────────────────────────
# Delete the transition guard test task
http DELETE "/tasks/$TASK2_ID" > /dev/null 2>&1 || true

# ── Summary ─────────────────────────────────────────────────────────────
echo ""
echo "════════════════════════════════════════════════════════════════"
echo "  SMOKE TEST RESULTS: $PASSED/$TOTAL passed, $FAILED failed"
echo "  User: $TEST_USER | API: $API_BASE | $(date -u +%Y-%m-%dT%H:%M:%SZ)"
echo "════════════════════════════════════════════════════════════════"

[[ "$FAILED" -eq 0 ]] && exit 0 || exit 1
