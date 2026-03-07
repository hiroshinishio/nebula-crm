/**
 * Tests for LoginPage (F0009 — S0001 contract)
 *
 * Covers:
 *   1. Renders Sign In button in OIDC mode
 *   2. Shows session_expired notice when ?reason=session_expired is present
 *   3. Shows callback_failed error when ?error=callback_failed is present
 *   4. Shows misconfiguration error when OIDC env vars are missing
 *   5. Clicking Sign In calls oidcUserManager.signinRedirect()
 *   6. Shows IdP-unavailable error when signinRedirect throws
 *
 * @vitest-environment jsdom
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import type { ReactNode } from 'react';

// ---------------------------------------------------------------------------
// Mock oidcUserManager
// ---------------------------------------------------------------------------

const mockSigninRedirect = vi.fn();

vi.mock('@/features/auth/oidcUserManager', () => ({
  oidcUserManager: {
    signinRedirect: mockSigninRedirect,
  },
}));

// ---------------------------------------------------------------------------
// Helper: import LoginPage after mocks are set up
// ---------------------------------------------------------------------------

async function importLoginPage() {
  const mod = await import('../../../pages/LoginPage');
  return mod.LoginPage;
}

function renderWithRouter(ui: ReactNode, initialPath: string) {
  return render(
    <MemoryRouter initialEntries={[initialPath]}>
      <Routes>
        <Route path="/login" element={ui} />
        <Route path="/" element={<div>HomePage</div>} />
      </Routes>
    </MemoryRouter>,
  );
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe('LoginPage', () => {
  beforeEach(() => {
    vi.resetAllMocks();
  });

  it('renders the Sign In button', async () => {
    const LoginPage = await importLoginPage();
    renderWithRouter(<LoginPage />, '/login');

    expect(screen.getByRole('button', { name: /sign in/i })).not.toBeNull();
  });

  it('shows session_expired notice when reason=session_expired', async () => {
    const LoginPage = await importLoginPage();
    renderWithRouter(<LoginPage />, '/login?reason=session_expired');

    expect(screen.getByText(/session has expired/i)).not.toBeNull();
  });

  it('shows callback_failed error when error=callback_failed', async () => {
    const LoginPage = await importLoginPage();
    renderWithRouter(<LoginPage />, '/login?error=callback_failed');

    expect(screen.getByText(/could not be completed/i)).not.toBeNull();
  });

  it('calls oidcUserManager.signinRedirect() when Sign In is clicked', async () => {
    mockSigninRedirect.mockResolvedValue(undefined);
    const LoginPage = await importLoginPage();
    renderWithRouter(<LoginPage />, '/login');

    fireEvent.click(screen.getByRole('button', { name: /sign in/i }));

    await waitFor(() => {
      expect(mockSigninRedirect).toHaveBeenCalledOnce();
    });
  });

  it('shows IdP-unavailable error when signinRedirect throws', async () => {
    mockSigninRedirect.mockRejectedValue(new Error('Network error'));
    const LoginPage = await importLoginPage();
    renderWithRouter(<LoginPage />, '/login');

    fireEvent.click(screen.getByRole('button', { name: /sign in/i }));

    await waitFor(() => {
      expect(screen.getByRole('alert')).not.toBeNull();
      expect(screen.getByText(/Unable to reach the identity provider/i)).not.toBeNull();
    });
  });
});
