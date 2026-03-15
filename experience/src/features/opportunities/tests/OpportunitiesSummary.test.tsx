/**
 * @vitest-environment jsdom
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';

const mockUseOpportunityFlow = vi.fn();
vi.mock('../hooks/useOpportunityFlow', () => ({
  useOpportunityFlow: (...args: unknown[]) => mockUseOpportunityFlow(...args),
}));

const mockUseOpportunityOutcomes = vi.fn();
vi.mock('../hooks/useOpportunityOutcomes', () => ({
  useOpportunityOutcomes: (...args: unknown[]) => mockUseOpportunityOutcomes(...args),
}));

const mockUseOpportunityAging = vi.fn();
vi.mock('../hooks/useOpportunityAging', () => ({
  useOpportunityAging: (...args: unknown[]) => mockUseOpportunityAging(...args),
}));

const mockUseOpportunityHierarchy = vi.fn();
vi.mock('../hooks/useOpportunityHierarchy', () => ({
  useOpportunityHierarchy: (...args: unknown[]) => mockUseOpportunityHierarchy(...args),
}));

const mockUseDashboardKpis = vi.fn();
vi.mock('@/features/kpis/hooks/useDashboardKpis', () => ({
  useDashboardKpis: (...args: unknown[]) => mockUseDashboardKpis(...args),
}));

import { OpportunitiesSummary } from '../components/OpportunitiesSummary';

const flowDto = {
  entityType: 'submission' as const,
  periodDays: 180,
  windowStartUtc: '2026-01-01T00:00:00Z',
  windowEndUtc: '2026-03-01T00:00:00Z',
  nodes: [
    {
      status: 'Received',
      label: 'Received',
      isTerminal: false,
      displayOrder: 1,
      colorGroup: 'intake' as const,
      currentCount: 10,
      inflowCount: 0,
      outflowCount: 7,
      avgDwellDays: 2.1,
      emphasis: 'normal' as const,
    },
    {
      status: 'Triaging',
      label: 'Triaging',
      isTerminal: false,
      displayOrder: 2,
      colorGroup: 'triage' as const,
      currentCount: 7,
      inflowCount: 8,
      outflowCount: 5,
      avgDwellDays: 5.4,
      emphasis: 'bottleneck' as const,
    },
    {
      status: 'InReview',
      label: 'In Review',
      isTerminal: false,
      displayOrder: 3,
      colorGroup: 'review' as const,
      currentCount: 4,
      inflowCount: 5,
      outflowCount: 4,
      avgDwellDays: 8.0,
      emphasis: 'blocked' as const,
    },
    {
      status: 'Bound',
      label: 'Bound',
      isTerminal: true,
      displayOrder: 4,
      colorGroup: 'decision' as const,
      currentCount: 15,
      inflowCount: 5,
      outflowCount: 0,
      avgDwellDays: null,
      emphasis: null,
    },
  ],
  links: [
    { sourceStatus: 'Received', targetStatus: 'Triaging', count: 12 },
    { sourceStatus: 'Triaging', targetStatus: 'InReview', count: 8 },
  ],
};

const outcomesDto = {
  periodDays: 180,
  totalExits: 20,
  outcomes: [
    {
      key: 'bound',
      label: 'Bound',
      branchStyle: 'solid' as const,
      count: 12,
      percentOfTotal: 60,
      averageDaysToExit: 7.2,
    },
    {
      key: 'declined',
      label: 'Declined',
      branchStyle: 'red_dashed' as const,
      count: 8,
      percentOfTotal: 40,
      averageDaysToExit: 5.8,
    },
  ],
};

describe('OpportunitiesSummary', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    mockUseOpportunityFlow.mockReturnValue({
      data: flowDto,
      isLoading: false,
      isError: false,
      refetch: vi.fn(),
    });
    mockUseOpportunityOutcomes.mockReturnValue({
      data: outcomesDto,
      isLoading: false,
      isError: false,
      refetch: vi.fn(),
    });
    mockUseOpportunityAging.mockReturnValue({
      data: null,
      isLoading: false,
      isError: false,
      refetch: vi.fn(),
    });
    mockUseOpportunityHierarchy.mockReturnValue({
      data: null,
      isLoading: false,
      isError: false,
      refetch: vi.fn(),
    });
    mockUseDashboardKpis.mockReturnValue({
      data: {
        activeBrokers: 10,
        openSubmissions: 7,
        renewalRate: 0.42,
        avgTurnaroundDays: 8.3,
      },
      isLoading: false,
      isError: false,
    });
  });

  it('renders chapter controls and connected flow canvas', () => {
    render(<OpportunitiesSummary />);

    expect(screen.getByRole('tab', { name: 'Flow' })).toBeTruthy();
    expect(screen.getByRole('tab', { name: 'Friction' })).toBeTruthy();
    expect(screen.getByRole('tab', { name: 'Outcomes' })).toBeTruthy();
    expect(screen.getByRole('tab', { name: 'Aging' })).toBeTruthy();
    expect(screen.getByRole('tab', { name: 'Mix' })).toBeTruthy();

    expect(screen.getByRole('button', { name: 'Received stage, 10 opportunities' })).toBeTruthy();
    expect(screen.getByRole('button', { name: /Bound outcome, 12 exits/i })).toBeTruthy();
  });

  it('passes periodDays to KPI hook', () => {
    render(<OpportunitiesSummary />);

    expect(mockUseDashboardKpis).toHaveBeenCalledWith(180);
    fireEvent.click(screen.getByRole('tab', { name: '30d' }));
    expect(mockUseDashboardKpis).toHaveBeenLastCalledWith(30);
  });

  it('lazy-loads aging data only when aging chapter is selected', async () => {
    render(<OpportunitiesSummary />);
    expect(mockUseOpportunityAging).toHaveBeenCalledWith('submission', 180, { enabled: false });

    fireEvent.click(screen.getByRole('tab', { name: 'Aging' }));

    await waitFor(() => {
      expect(mockUseOpportunityAging).toHaveBeenLastCalledWith('submission', 180, { enabled: true });
    });
  });

  it('lazy-loads mix data only when mix chapter is selected', async () => {
    render(<OpportunitiesSummary />);
    expect(mockUseOpportunityHierarchy).toHaveBeenCalledWith(180, { enabled: false });

    fireEvent.click(screen.getByRole('tab', { name: 'Mix' }));

    await waitFor(() => {
      expect(mockUseOpportunityHierarchy).toHaveBeenLastCalledWith(180, { enabled: true });
    });
  });

  it('shows flow error fallback', () => {
    mockUseOpportunityFlow.mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: true,
      refetch: vi.fn(),
    });

    render(<OpportunitiesSummary />);

    expect(screen.getByText('Unable to load opportunity flow')).toBeTruthy();
  });
});
