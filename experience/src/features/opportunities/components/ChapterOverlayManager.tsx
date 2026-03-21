import type { StoryChapter } from './storyTypes';
import type { OutcomeAnchor, StageAnchor } from './storyTimelineTypes';
import { FrictionOverlay } from './overlays/FrictionOverlay';
import { OutcomesOverlay } from './overlays/OutcomesOverlay';
import { ErrorFallback } from '@/components/ui/ErrorFallback';
import { Skeleton } from '@/components/ui/Skeleton';

interface ChapterOverlayManagerProps {
  chapter: StoryChapter;
  stageAnchors: StageAnchor[];
  outcomeAnchors: OutcomeAnchor[];
  outcomesLoading: boolean;
  outcomesError: boolean;
  onRetryOutcomes: () => void;
}

export function ChapterOverlayManager({
  chapter,
  stageAnchors,
  outcomeAnchors,
  outcomesLoading,
  outcomesError,
  onRetryOutcomes,
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
  return null;
}
