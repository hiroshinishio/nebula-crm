import { getDevToken } from './dev-auth'
import { emitAuthEvent } from '@/features/auth/authEvents'

const API_BASE = ''

interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  code?: string
  traceId?: string
  errors?: Record<string, string[]>
}

export class ApiError extends Error {
  constructor(
    public status: number,
    public problem: ProblemDetails | null,
  ) {
    super(problem?.title ?? `HTTP ${status}`)
    this.name = 'ApiError'
  }

  get code(): string | undefined {
    return this.problem?.code
  }

  get traceId(): string | undefined {
    return this.problem?.traceId
  }
}

/**
 * 401/403 interceptor: emits auth events so useAuthEventHandler (mounted in
 * AppInner) can execute navigation without coupling api.ts to React Router.
 *
 * - 401 → emits 'session_expired' → session teardown → /login?reason=session_expired
 * - 403 with code='broker_scope_unresolvable' → emits 'broker_scope_unresolvable'
 *   → navigate to /unauthorized?reason=broker_inactive (no session teardown —
 *   the JWT is valid; only the broker scope mapping is missing/deactivated).
 * - All other errors → re-thrown as ApiError for callers to handle.
 *
 * The interceptor never throws for the two emitted cases — navigation is in
 * flight via the event bus and callers receive a never-resolving promise so
 * TanStack Query does not process a stale result during redirect.
 */
function handleErrorIntercept(status: number, problem: ProblemDetails | null): void {
  if (status === 401) {
    emitAuthEvent('session_expired')
  } else if (status === 403 && problem?.code === 'broker_scope_unresolvable') {
    emitAuthEvent('broker_scope_unresolvable')
  } else {
    throw new ApiError(status, problem)
  }
}

async function resolveToken(): Promise<string> {
  // In OIDC mode the token will be sourced from oidcUserManager (F0009-S0001).
  // For now, fall through to dev token so existing behaviour is unchanged.
  return getDevToken()
}

async function fetchApi<T>(path: string, options?: RequestInit): Promise<T> {
  const token = await resolveToken()
  const response = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
      ...options?.headers,
    },
    credentials: 'include',
  })

  if (!response.ok) {
    const problem = await response.json().catch(() => null)
    handleErrorIntercept(response.status, problem)
    // Execution reaches here only when an auth event was emitted (401 or
    // broker_scope_unresolvable 403): navigation is in flight via the event
    // bus. Return a promise that never resolves so downstream TanStack Query
    // callers don't process a stale result while the app is redirecting away.
    // eslint-disable-next-line @typescript-eslint/no-empty-function
    return new Promise<T>(() => {})
  }

  return response.json()
}

async function fetchApiNoBody(path: string, options?: RequestInit): Promise<void> {
  const token = await resolveToken()
  const response = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
      ...options?.headers,
    },
    credentials: 'include',
  })

  if (!response.ok) {
    const problem = await response.json().catch(() => null)
    handleErrorIntercept(response.status, problem)
  }
}

export const api = {
  get: <T>(path: string) => fetchApi<T>(path),
  post: <T>(path: string, body: unknown) =>
    fetchApi<T>(path, { method: 'POST', body: JSON.stringify(body) }),
  put: <T>(path: string, body: unknown, headers?: Record<string, string>) =>
    fetchApi<T>(path, { method: 'PUT', body: JSON.stringify(body), headers }),
  delete: (path: string) =>
    fetchApiNoBody(path, { method: 'DELETE' }),
}
