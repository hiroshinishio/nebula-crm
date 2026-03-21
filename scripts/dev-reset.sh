#!/usr/bin/env bash
# F0014: Clean teardown + rebuild + smoke test in one command.
# This is the canonical "prove it works from scratch" workflow
# for DevOps verification of any feature.
#
# Usage:
#   ./scripts/dev-reset.sh                   # Full reset + smoke test
#   ./scripts/dev-reset.sh --skip-smoke      # Reset only, no smoke test
#   ./scripts/dev-reset.sh --user john.miller # Reset + smoke test as specific user
#
# What it does:
#   1. docker compose down -v  (removes containers AND volumes — clean DB)
#   2. docker compose up -d --build  (rebuild images, start fresh)
#   3. Wait for all services healthy
#   4. Run smoke-test.sh (unless --skip-smoke)
#
# Exit codes:
#   0 — stack healthy + smoke tests passed
#   1 — smoke test failure
#   2 — infrastructure failure (services didn't come up)

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
SKIP_SMOKE=false
SMOKE_ARGS=()

while [[ $# -gt 0 ]]; do
  case "$1" in
    --skip-smoke)  SKIP_SMOKE=true; shift ;;
    --user)        SMOKE_ARGS+=(--user "$2"); shift 2 ;;
    --help|-h)
      sed -n '2,/^$/p' "$0" | sed 's/^# \?//'
      exit 0 ;;
    *) echo "Unknown option: $1"; exit 2 ;;
  esac
done

cd "$PROJECT_DIR"

echo "════════════════════════════════════════════════════════════════"
echo "  NEBULA DEV RESET — $(date -u +%Y-%m-%dT%H:%M:%SZ)"
echo "════════════════════════════════════════════════════════════════"
echo ""

# ── Step 1: Tear down ──────────────────────────────────────────────────
echo "==> Step 1: Tearing down stack (containers + volumes)..."
docker compose down -v 2>&1 | tail -5
echo ""

# ── Step 2: Rebuild + start ────────────────────────────────────────────
echo "==> Step 2: Building images and starting services..."
docker compose up -d --build 2>&1 | tail -10
echo ""

# ── Step 3: Wait for health ────────────────────────────────────────────
echo "==> Step 3: Waiting for services to become healthy..."

wait_healthy() {
  local service="$1" max_attempts="${2:-60}"
  for i in $(seq 1 "$max_attempts"); do
    local status
    status=$(docker compose ps --format '{{.Status}}' "$service" 2>/dev/null || echo "")
    if [[ "$status" == *"healthy"* ]] || [[ "$status" == "Up"* && "$service" == "api" ]]; then
      echo "  $service: healthy (attempt $i)"
      return 0
    fi
    sleep 2
  done
  echo "  TIMEOUT: $service not healthy after $((max_attempts * 2))s"
  echo "  Last status: $status"
  echo "  Logs:"
  docker compose logs "$service" --tail 15 2>&1 | sed 's/^/    /'
  return 1
}

wait_healthy db 30 || exit 2
wait_healthy authentik-server 60 || exit 2
wait_healthy authentik-worker 60 || exit 2

# API doesn't have a healthcheck in compose, so poll the endpoint
echo "  Waiting for API healthz..."
for i in $(seq 1 45); do
  if curl -sf -o /dev/null "http://localhost:8080/healthz" 2>/dev/null; then
    echo "  api: healthy (attempt $i)"
    break
  fi
  if [[ $i -eq 45 ]]; then
    echo "  TIMEOUT: API not ready"
    docker compose logs api --tail 15 2>&1 | sed 's/^/    /'
    exit 2
  fi
  sleep 2
done
echo ""

# Wait a few seconds for authentik blueprints to be fully applied by the worker
echo "  Waiting for blueprint application..."
sleep 5

# ── Step 4: Service summary ────────────────────────────────────────────
echo "==> Service status:"
docker compose ps --format "table {{.Name}}\t{{.Status}}" 2>&1
echo ""

# ── Step 5: Smoke test ─────────────────────────────────────────────────
if [[ "$SKIP_SMOKE" == "true" ]]; then
  echo "==> Smoke test skipped (--skip-smoke)"
  echo ""
  echo "  Stack is ready. Run smoke tests manually:"
  echo "    ./scripts/smoke-test.sh"
  exit 0
fi

echo "==> Step 4: Running smoke tests..."
echo ""
exec "$SCRIPT_DIR/smoke-test.sh" "${SMOKE_ARGS[@]}"
