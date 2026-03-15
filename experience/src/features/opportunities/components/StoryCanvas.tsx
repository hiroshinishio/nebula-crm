import { useState } from 'react';
import { ErrorFallback } from '@/components/ui/ErrorFallback';
import { Skeleton } from '@/components/ui/Skeleton';
import { KpiCardsRow } from '@/features/kpis';
import { cn } from '@/lib/utils';
import { useOpportunityAging } from '../hooks/useOpportunityAging';
import { useOpportunityFlow } from '../hooks/useOpportunityFlow';
import { useOpportunityHierarchy } from '../hooks/useOpportunityHierarchy';
import { useOpportunityOutcomes } from '../hooks/useOpportunityOutcomes';
import { ConnectedFlow } from './ConnectedFlow';
import type { StoryChapter } from './storyTypes';

const PERIOD_WINDOWS = [30, 90, 180, 365] as const;

const CHAPTERS: { key: StoryChapter; label: string }[] = [
  { key: 'flow', label: 'Flow' },
  { key: 'friction', label: 'Friction' },
  { key: 'outcomes', label: 'Outcomes' },
  { key: 'aging', label: 'Aging' },
  { key: 'mix', label: 'Mix' },
];

export function StoryCanvas() {
  const [periodDays, setPeriodDays] = useState<(typeof PERIOD_WINDOWS)[number]>(180);
  const [chapter, setChapter] = useState<StoryChapter>('flow');

  const flowQuery = useOpportunityFlow('submission', periodDays);
  const outcomesQuery = useOpportunityOutcomes(periodDays);
  const agingQuery = useOpportunityAging('submission', periodDays, {
    enabled: chapter === 'aging',
  });
  const mixQuery = useOpportunityHierarchy(periodDays, {
    enabled: chapter === 'mix',
  });

  return (
    <section aria-label="Opportunity story canvas">
      <header className="canvas-section canvas-zone-tight" aria-label="Story controls">
        <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
          <div
            className="inline-flex flex-wrap items-center gap-1"
            role="tablist"
            aria-label="Opportunity period window"
          >
            {PERIOD_WINDOWS.map((windowDays) => {
              const active = windowDays === periodDays;
              return (
                <button
                  key={windowDays}
                  type="button"
                  role="tab"
                  aria-selected={active}
                  aria-controls="story-flow-canvas"
                  onClick={() => setPeriodDays(windowDays)}
                  className={cn(
                    'rounded-full bg-surface-main/55 px-3 py-1 text-xs font-semibold text-text-secondary transition-colors',
                    active && 'bg-nebula-violet/20 text-nebula-violet',
                  )}
                >
                  {windowDays}d
                </button>
              );
            })}
          </div>

          <div
            className="inline-flex flex-wrap items-center gap-1"
            role="tablist"
            aria-label="Story chapters"
          >
            {CHAPTERS.map((chapterOption) => {
              const active = chapterOption.key === chapter;
              return (
                <button
                  key={chapterOption.key}
                  type="button"
                  role="tab"
                  aria-selected={active}
                  aria-controls="story-flow-canvas"
                  onClick={() => setChapter(chapterOption.key)}
                  className={cn(
                    'rounded-full bg-surface-main/55 px-3 py-1 text-xs font-semibold text-text-secondary transition-colors',
                    active && 'bg-nebula-violet/20 text-nebula-violet',
                  )}
                >
                  {chapterOption.label}
                </button>
              );
            })}
          </div>
        </div>
      </header>

      <KpiCardsRow periodDays={periodDays} className="canvas-zone-tight" />

      <section id="story-flow-canvas" className="canvas-section canvas-zone-default">
        {flowQuery.isLoading && <Skeleton className="h-[320px] w-full" />}

        {flowQuery.isError && (
          <ErrorFallback
            message="Unable to load opportunity flow"
            onRetry={() => flowQuery.refetch()}
          />
        )}

        {flowQuery.data && (
          <ConnectedFlow
            flow={flowQuery.data}
            outcomes={outcomesQuery.data?.outcomes ?? []}
            chapter={chapter}
            periodDays={periodDays}
            outcomesLoading={outcomesQuery.isLoading}
            outcomesError={outcomesQuery.isError}
            onRetryOutcomes={() => outcomesQuery.refetch()}
            agingData={agingQuery.data}
            agingLoading={agingQuery.isLoading}
            agingError={agingQuery.isError}
            onRetryAging={() => agingQuery.refetch()}
            mixData={mixQuery.data}
            mixLoading={mixQuery.isLoading}
            mixError={mixQuery.isError}
            onRetryMix={() => mixQuery.refetch()}
          />
        )}
      </section>
    </section>
  );
}
