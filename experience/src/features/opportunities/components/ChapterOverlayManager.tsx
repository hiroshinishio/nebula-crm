import type { OpportunityAgingDto, OpportunityHierarchyDto } from '../types';
import type { OutcomeAnchor, StageAnchor } from './ConnectedFlow';
import type { StoryChapter } from './storyTypes';
import { FrictionOverlay } from './overlays/FrictionOverlay';
import { OutcomesOverlay } from './overlays/OutcomesOverlay';
import { AgingOverlay } from './overlays/AgingOverlay';
import { MixOverlay } from './overlays/MixOverlay';
import { ErrorFallback } from '@/components/ui/ErrorFallback';
import { Skeleton } from '@/components/ui/Skeleton';

interface ChapterOverlayManagerProps {
  chapter: StoryChapter;
  stageAnchors: StageAnchor[];
  outcomeAnchors: OutcomeAnchor[];
  outcomesLoading: boolean;
  outcomesError: boolean;
  onRetryOutcomes: () => void;
  agingData?: OpportunityAgingDto;
  agingLoading: boolean;
  agingError: boolean;
  onRetryAging: () => void;
  mixData?: OpportunityHierarchyDto;
  mixLoading: boolean;
  mixError: boolean;
  onRetryMix: () => void;
}

export function ChapterOverlayManager({
  chapter,
  stageAnchors,
  outcomeAnchors,
  outcomesLoading,
  outcomesError,
  onRetryOutcomes,
  agingData,
  agingLoading,
  agingError,
  onRetryAging,
  mixData,
  mixLoading,
  mixError,
  onRetryMix,
}: ChapterOverlayManagerProps) {
  if (chapter === 'flow') {
    return null;
  }

  if (chapter === 'friction') {
    return <FrictionOverlay stageAnchors={stageAnchors} />;
  }

  if (chapter === 'outcomes') {
    if (outcomesLoading) {
      return (
        <div className="pointer-events-none absolute inset-x-4 bottom-3">
          <Skeleton className="h-14 w-full" />
        </div>
      );
    }

    if (outcomesError) {
      return (
        <div className="absolute inset-x-4 bottom-3 pointer-events-auto">
          <ErrorFallback message="Unable to load outcomes overlay data" onRetry={onRetryOutcomes} />
        </div>
      );
    }

    return <OutcomesOverlay outcomeAnchors={outcomeAnchors} />;
  }

  if (chapter === 'aging') {
    return (
      <AgingOverlay
        data={agingData}
        isLoading={agingLoading}
        isError={agingError}
        onRetry={onRetryAging}
      />
    );
  }

  return (
    <MixOverlay
      data={mixData}
      isLoading={mixLoading}
      isError={mixError}
      onRetry={onRetryMix}
    />
  );
}
